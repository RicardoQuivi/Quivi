import { useTranslation } from "react-i18next";
import { Modal, ModalSize } from "../../../../components/ui/modal"
import { ModalButtonsFooter } from "../../../../components/ui/modal/ModalButtonsFooter";
import { Printer } from "../../../../hooks/api/Dtos/printers/Printer";
import Button from "../../../../components/ui/button/Button";
import { usePrinterMessageMutator } from "../../../../hooks/mutators/usePrinterMessageMutator";
import ResponsiveTable from "../../../../components/tables/ResponsiveTable";
import { Divider } from "../../../../components/dividers/Divider";
import { QueryPagination } from "../../../../components/pagination/QueryPagination";
import { useEffect, useMemo, useState } from "react";
import { usePrinterMessagesQuery } from "../../../../hooks/queries/implementations/usePrinterMessagesQuery";
import { PrinterMessageStatus } from "../../../../hooks/api/Dtos/printerMessages/PrinterMessageStatus";
import { NotificationType } from "../../../../hooks/api/Dtos/notifications/NotificationType";
import Badge from "../../../../components/ui/badge/Badge";
import { PrinterMessage } from "../../../../hooks/api/Dtos/printerMessages/PrinterMessage";
import { Tooltip } from "../../../../components/ui/tooltip/Tooltip";
import { useToast } from "../../../../layout/ToastProvider";
import { Spinner } from "../../../../components/spinners/Spinner";

interface Props {
    readonly printer?: Printer;
    readonly onClose: () => any;
}
export const PrinterAuditModal = (props: Props) => {
    const { t, i18n } = useTranslation();
    const toast = useToast();
    const mutator = usePrinterMessageMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 5,
    })
    const [isLoading, setIsLoading] = useState(false);

    const messagesQuery = usePrinterMessagesQuery(props.printer == undefined ? undefined : {
        printerId: props.printer.id,
        page: state.page,
        pageSize: state.pageSize,
    })

    const getTitle = () => {
        if(props.printer == undefined) {
            return;
        }

        return t("pages.printers.printings", {
            name: props.printer.name
        })
    }

    const formatDate = (date: string) => new Date(date).toLocaleString(i18n.language, {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
    });

    useEffect(() => setState(s => ({ ...s, page: 0 })), [props.printer])

    const notificationNamesMap = useMemo(() => Object.entries(NotificationType)
        .filter(([_, value]) => !isNaN(Number(value))) // only numeric entries
        .map(([key, value]) => [Number(value), key] as [NotificationType, string])
        .reduce((map, e) => {
            map.set(e[0], e[1])
            return map;
        }, new Map<NotificationType, string>())
    , [])

    const print = async (printer: Printer | undefined, pingOnly: boolean) => {
        if(printer == undefined) {
            return;
        }

        setIsLoading(true);
        try {
            await mutator.create({
                printerId: printer.id,
                text: t("pages.printers.textMessage"),
                pingOnly: pingOnly,
            })
        } catch {
            toast.error(t("common.operations.failure.generic"));
        } finally {
            setIsLoading(false);
        }
    }

    const getState = (message: PrinterMessage) => {
        let status = PrinterMessageStatus.Success;
        let at = message.statuses[status];

        if(at == undefined) {
            status = PrinterMessageStatus.Failed;
            at = message.statuses[status];
        }
        if(at == undefined) {
            status = PrinterMessageStatus.TimedOut;
            at = message.statuses[status];
        }
        if(at == undefined) {
            status = PrinterMessageStatus.Unreachable;
            at = message.statuses[status];
        }

        if(at != undefined) {
            return <Tooltip
                    message={formatDate(at)}
                >
                <Badge 
                    variant="solid"
                    color={status == PrinterMessageStatus.Success ? "success" : "error"}
                >
                    {t(`common.printerMessageStatus.${PrinterMessageStatus[status]}`)}
                </Badge>
            </Tooltip>
        }

        status = PrinterMessageStatus.Processing;
        at = message.statuses[status];
        if(at !=undefined) {
            return <Tooltip
                    message={formatDate(at)}
                >
                <Badge 
                    variant="light"
                    color="info"
                    startIcon={<Spinner />}
                >
                    {t(`common.printerMessageStatus.${PrinterMessageStatus[status]}`)}
                </Badge>
            </Tooltip>
        }

        return <Tooltip
            message={formatDate(message.statuses[PrinterMessageStatus.Created])}
        >
            <Badge
                variant="light"
                color="info"
                startIcon={<Spinner />}
            >
                {t(`pages.printers.messages.awaitingReception`)}
            </Badge>
        </Tooltip>
    }

    return <Modal
        isOpen={props.printer != undefined}
        size={ModalSize.Default}
        onClose={props.onClose}
        title={getTitle()}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        <div className="grid grid-cols-[auto_auto] gap-2 w-full">
            <Button
                size="sm"
                variant="outline"
                onClick={() => print(props.printer, true)}
                disabled={isLoading}
            >
                {
                    isLoading
                    ?
                    <Spinner />
                    :
                    t("pages.printers.pingPrinter")
                }
            </Button>
            <Button
                size="sm"
                variant="outline"
                onClick={() => print(props.printer, false)}
                disabled={isLoading}
            >
                {
                    isLoading
                    ?
                    <Spinner />
                    :
                    t("pages.printers.printTest")
                }
            </Button>
        </div>
        <ResponsiveTable
            isLoading={messagesQuery.isFirstLoading}
            columns={[
                {
                    key: "type",
                    label: t("pages.printers.type"),
                    render: item => <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                        {t(`common.notificationTypes.${notificationNamesMap.get(item.type)}`)}
                    </span>
                },
                {
                    key: "createdAt",
                    label: t("pages.printers.messages.createdAt"),
                    render: item => (<span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                        {formatDate(item.statuses[PrinterMessageStatus.Created])}
                    </span>)
                },
                {
                    key: "processingAt",
                    label: t("pages.printers.messages.status"),
                    render: getState,
                },
            ]}
            data={messagesQuery.data}
            getKey={d => `${d.printerId}-${d.messageId}`}
        />
        <Divider />
        <QueryPagination
            query={messagesQuery}
            onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
            pageSize={state.pageSize}
        />
    </Modal>
}