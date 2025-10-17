import { useTranslation } from "react-i18next";
import { Spinner } from "../../../components/spinners/Spinner";
import { Modal, ModalSize } from "../../../components/ui/modal/Modal"
import { ModalButtonsFooter } from "../../../components/ui/modal/ModalButtonsFooter";
import { useToast } from "../../../layout/ToastProvider";
import { useEffect, useMemo, useState } from "react";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { MultiSelectionZone } from "../../../components/inputs/MultiSelectionZone";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { Availability } from "../../../hooks/api/Dtos/availabilities/Availability";
import { Collections } from "../../../utilities/Collectionts";
import { useAvailabilityChannelProfileAssociationMutator } from "../../../hooks/mutators/useAvailabilityChannelProfileAssociationMutator";
import { useAvailabilityChannelProfilesAssociationsQuery } from "../../../hooks/queries/implementations/useAvailabilityChannelProfilesAssociationsQuery";
import { ChannelProfile } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { UpdateAvailabilityChannelProfileAssociation } from "../../../hooks/api/Dtos/availabilityChannelProfileAssociations/UpdateAvailabilityChannelProfileAssociationsRequest";

const pageSize = 12;

interface Props {
    readonly model: Availability | undefined;
    readonly onClose: () => void;
}
export const LinkToChannelProfilesModal = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const mutator = useAvailabilityChannelProfileAssociationMutator();
    
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [page, setPage] = useState(0);

    const associationsQuery = useAvailabilityChannelProfilesAssociationsQuery(props.model == undefined ? undefined : {
        availabilityIds: [props.model.id],
        page: 0,
    })
    const channelProfilesQuery = useChannelProfilesQuery({
        page: page,
        pageSize: pageSize,
    })

    const [toggles, setToggles] = useState(() => new Set<string>());

    const associations = useMemo(() => Collections.toSet(associationsQuery.data, a => a.channelProfileId), [associationsQuery.data])
    
    const selected = useMemo(() => {
        const set = new Set<string>(associations);

        for(const t of toggles.keys()) {
            if(set.has(t)) {
                set.delete(t);
                continue;
            }

            set.add(t);
        }

        const result = [] as ChannelProfile[];
        for(const p of channelProfilesQuery.data) {
            if(set.has(p.id) == false) {
                continue;
            }

            result.push(p);
        }

        return result;
    }, [toggles, associations, channelProfilesQuery.data])

    useEffect(() => {
        if(props.model == undefined) {
            return;
        }

        setToggles(new Set<string>());
    }, [props.model])

    const save = async () => {
        if(props.model == undefined) {
            return;
        }

        try {
            setIsSubmitting(true);
            if(toggles.size != 0) {
                const aux = [] as UpdateAvailabilityChannelProfileAssociation[];

                for(const a of associations) {
                    if(toggles.has(a)) {
                        aux.push({
                            id: a,
                            active: false,
                        })
                    }
                }

                for(const a of toggles) {
                    if(associations.has(a) == false) {
                        aux.push({
                            id: a,
                            active: true,
                        })
                    }
                }

                await mutator.patchAvailability(props.model, associationsQuery.data, {
                    associations: aux,
                })
            }
            toast.success(t("common.operations.success.edit"));
        } catch {
            toast.error(t("common.operations.failure.generic"));
        } finally {
            props.onClose();
            setIsSubmitting(false);
        }
    }

    return <Modal
        isOpen={props.model != undefined}
        onClose={props.onClose}
        size={ModalSize.Medium}
        title={t("pages.availabilities.linkToChannelProfiles", {
            name: props.model?.name ?? <Skeleton />,
        })}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: isSubmitting
                                ?
                                <Spinner />
                                :
                                t("common.confirm"),
                    disabled: isSubmitting || toggles.size == 0,
                    onClick: save,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        <div>
            {
                channelProfilesQuery.isFirstLoading || associationsQuery.isFirstLoading
                ?
                <MultiSelectionZone
                    options={[1, 2, 3, 4, 5, 6]}
                    selected={[1, 2, 3, 4, 5, 6]}
                    getId={s => `Loading-${s}`}
                    render={_ => <h5 className="text-sm font-medium text-gray-800 dark:text-white/90 w-full">
                        <Skeleton className="w-full" />
                    </h5>}
                    checkIcon={Spinner}
                />
                :
                <MultiSelectionZone
                    options={channelProfilesQuery.data}
                    selected={selected}
                    getId={s => s.id}
                    render={s => <h5 className="text-sm font-medium text-gray-800 dark:text-white/90">{s.name}</h5>}
                    onChange={(_, d) => setToggles(t => {
                        const result = new Set<string>(t);

                        if(result.has(d.id)) {
                            result.delete(d.id)
                        } else {
                            result.add(d.id);
                        }

                        return result;
                    })}
                />
            }
            <br/>
            <QueryPagination
                query={channelProfilesQuery}
                onPageIndexChange={setPage}
                pageSize={pageSize}
            />
        </div>
    </Modal>
}