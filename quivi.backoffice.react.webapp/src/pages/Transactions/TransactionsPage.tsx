import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useMemo, useState } from "react";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import ResponsiveTable from "../../components/tables/ResponsiveTable";
import { Tooltip } from "../../components/ui/tooltip/Tooltip";
import { CommentIcon, FeesIcon, QuiviIcon, StarIcon, SyncIcon } from "../../icons";
import { Divider } from "../../components/dividers/Divider";
import { QueryPagination } from "../../components/pagination/QueryPagination";
import { SynchronizationState, Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useCustomChargeMethodsQuery } from "../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { CustomChargeMethod } from "../../hooks/api/Dtos/customchargemethods/CustomChargeMethod";
import { Spinner } from "../../components/spinners/Spinner";
import Avatar from "../../components/ui/avatar/Avatar";
import { useMerchantsQuery } from "../../hooks/queries/implementations/useMerchantsQuery";
import { Merchant } from "../../hooks/api/Dtos/merchants/Merchant";
import { Skeleton } from "../../components/ui/skeleton/Skeleton";
import { PublicId } from "../../components/publicids/PublicId";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { Channel } from "../../hooks/api/Dtos/channels/Channel";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { ChannelProfile } from "../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { DateUtils } from "../../utilities/dateutils";
import CurrencySpan from "../../components/currency/CurrencySpan";
import { useAuthenticatedUser } from "../../context/AuthContext";
import Badge from "../../components/ui/badge/Badge";
import { useReviewsQuery } from "../../hooks/queries/implementations/useReviewsQuery";
import { Review } from "../../hooks/api/Dtos/reviews/Review";
import { TransactionModal } from "./TransactionModal";

interface Props {
    readonly isAdmin?: boolean;
}
export const TransactionsPage = (props: Props) => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const user = useAuthenticatedUser();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
    })
    const transactionsQuery = useTransactionsQuery({
        adminView: props.isAdmin,
        page: state.page,
        pageSize: state.pageSize,
    })

    const customChargeMethodIds = useMemo(() => {
        const set = new Set<string>();
        for(const t of transactionsQuery.data) {
            if(t.customChargeMethodId == undefined) {
                continue;
            }

            set.add(t.customChargeMethodId);
        }

        return Array.from(set.values())
    }, [transactionsQuery.data])
    const customChargeMethodsQuery = useCustomChargeMethodsQuery({
        ids: customChargeMethodIds,
        page: 0,
        pageSize: undefined,
    })
    const customChargeMethodsMap = useMemo(() => {
        const result = new Map<string, CustomChargeMethod>();
        for(const c of customChargeMethodsQuery.data) {
            result.set(c.id, c);
        }
        return result
    }, [customChargeMethodsQuery.data])


    const merchantIds = useMemo(() => {
        const set = new Set<string>();
        for(const t of transactionsQuery.data) {
            set.add(t.merchantId);
        }
        return Array.from(set.values())
    }, [transactionsQuery.data])
    const merchantsQuery = useMerchantsQuery({
        ids: merchantIds,
        page: 0,
        pageSize: undefined,
    })
    const merchantsMap = useMemo(() => {
        const result = new Map<string, Merchant>();
        for(const c of merchantsQuery.data) {
            result.set(c.id, c);
        }
        return result
    }, [merchantsQuery.data])


    const channelIds = useMemo(() => {
        const set = new Set<string>();
        for(const t of transactionsQuery.data) {
            set.add(t.channelId);
        }
        return Array.from(set.values())
    }, [transactionsQuery.data])
    const channelsQuery = useChannelsQuery({
        ids: channelIds,
        page: 0,
        pageSize: undefined,
    })
    const channelsMap = useMemo(() => {
        const result = new Map<string, Channel>();
        for(const c of channelsQuery.data) {
            result.set(c.id, c);
        }
        return result
    }, [channelsQuery.data])


    const profileIds = useMemo(() => {
        const set = new Set<string>();
        for(const t of channelsQuery.data) {
            set.add(t.channelProfileId);
        }
        return Array.from(set.values())
    }, [channelsQuery.data])
    const profilesQuery = useChannelProfilesQuery({
        ids: profileIds,
        page: 0,
        pageSize: undefined,
    })
    const profilesMap = useMemo(() => {
        const result = new Map<string, ChannelProfile>();
        for(const c of profilesQuery.data) {
            result.set(c.id, c);
        }
        return result
    }, [profilesQuery.data])


    const reviewIds = useMemo(() => {
        const set = new Set<string>();
        for(const t of transactionsQuery.data) {
            set.add(t.id);
        }
        return Array.from(set.values())
    }, [transactionsQuery.data])
    const reviewsQuery = useReviewsQuery({
        ids: reviewIds,
        page: 0,
        pageSize: undefined,
    })
    const reviewsMap = useMemo(() => {
        const result = new Map<string, Review>();
        for(const c of reviewsQuery.data) {
            result.set(c.id, c);
        }
        return result
    }, [reviewsQuery.data])
    
    const getIcon = (t: Transaction) => {
        if(t.customChargeMethodId == undefined) {
            return <Avatar src={<QuiviIcon className="h-full w-auto" />} size="medium" alt="Quivi" />
        }
        
        const customCharge = customChargeMethodsMap.get(t.customChargeMethodId);
        if(customCharge == undefined) {
            return <Avatar src={<Spinner className="h-full w-auto" />} size="medium" alt="Loading" />
        }

        return <Avatar src={customCharge.logoUrl} size="medium" alt={customCharge.name} />
    }

    const getMerchantName = (t: Transaction) => {
        const merchant = merchantsMap.get(t.merchantId);
        if(merchant == undefined) {
            return <Skeleton />
        }
        return <b>{merchant.name}</b>;
    }

    const getChannelName = (t: Transaction) => {
        const channel = channelsMap.get(t.channelId);
        if(channel == undefined) {
            return <Skeleton />
        }

        const profile = profilesMap.get(channel.channelProfileId);
        if(profile == undefined) {
            return <Skeleton />
        }
        return <b>{profile.name} {channel.name}</b>;
    }

    return <>
        <PageMeta
            title={t("pages.transactions.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.transactions.title")} />

        <ComponentCard title={t("pages.transactions.title")}>
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white pt-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={transactionsQuery.isFirstLoading}
                        columns={[
                            {
                                key: "icon",
                                render: getIcon,
                                label: <></>
                            },
                            {
                                key: "id",
                                render: (d) => <PublicId id={d.id} />,
                                label: t("common.id"),
                            },
                            {
                                key: "merchant",
                                render: getMerchantName,
                                label: t("common.entities.merchant"),
                            },
                            {
                                key: "channel",
                                render: getChannelName,
                                label: t("common.entities.channel"),
                            },
                            {
                                key: "total",
                                render: d => (
                                    <div
                                        className="flex items-center gap-3"
                                    >
                                        <CurrencySpan value={d.payment + d.tip} />
                                        {
                                            user.isAdmin && d.surcharge > 0 &&
                                            <Tooltip message={t("common.surcharge")}>
                                                <Badge 
                                                    variant="solid"
                                                    color="warning"
                                                    endIcon={<FeesIcon />}
                                                >
                                                    + <CurrencySpan value={d.surcharge}/>
                                                </Badge>
                                            </Tooltip>
                                        }
                                    </div>
                                ),
                                label: t("common.total"),
                            },
                            {
                                key: "amount",
                                render: d => <CurrencySpan value={d.payment} />,
                                label: t("common.currencyAmount"),
                            },
                            {
                                key: "discount",
                                render: d => d.paymentDiscount == undefined ? t("common.notApplicable") : <CurrencySpan value={d.paymentDiscount} />,
                                label: t("common.discount"),
                            },
                            {
                                key: "tip",
                                render: d => <CurrencySpan value={d.tip} />,
                                label: t("common.tip"),
                            },
                            {
                                key: "date",
                                render: d => DateUtils.toString(d.capturedDate),
                                label: t("common.date"),
                            },
                            {
                                key: "additionalInfo",
                                render: d => {
                                    const review = reviewsMap.get(d.id);
                                    return (
                                        <div
                                            className="flex items-center gap-3"
                                        >
                                            {
                                                review != null &&
                                                <>
                                                    <Tooltip message={t("common.entities.review")}>
                                                        <Badge 
                                                            variant="solid"
                                                            color={review.stars > 3 ? "success" : "warning"}
                                                            startIcon={<StarIcon fill="currentColor" />}
                                                        >
                                                            {review.stars}
                                                        </Badge>
                                                    </Tooltip>

                                                    {
                                                        review.comment != undefined &&
                                                        <Tooltip message={t("common.comment")}>
                                                            <Badge
                                                                variant="solid"
                                                                color="light"
                                                            >
                                                                &nbsp;<CommentIcon className="h-full" fill="currentColor" />&nbsp;
                                                            </Badge>
                                                        </Tooltip>
                                                    }
                                                </>
                                            }
                                            {
                                                d.syncingState == SynchronizationState.Syncing
                                                ?
                                                <Tooltip message={t("common.syncing")}>
                                                    <Badge 
                                                        variant="solid"
                                                        color="light"
                                                    >
                                                        &nbsp;<Spinner className="h-full" />&nbsp;
                                                    </Badge>
                                                </Tooltip>
                                                :
                                                (
                                                    d.syncingState == SynchronizationState.Failed &&
                                                    <Tooltip message={t("pages.transactions.syncingError")}>
                                                        <Badge
                                                            variant="solid"
                                                            color="error"
                                                        >
                                                            &nbsp;<SyncIcon className="h-full" fill="currentColor" />&nbsp;
                                                        </Badge>
                                                    </Tooltip>
                                                )
                                            }
                                        </div>
                                    );
                                },
                                label: t("common.additionalInfo"),
                            },
                        ]}
                        data={transactionsQuery.data}
                        getKey={d => d.id}
                        onRowClick={d => {
                            if(props.isAdmin == true) {
                                navigate(`/admin/transactions/${d.id}`);
                                return;
                            }
                            navigate(`/transactions/${d.id}`);
                        }}
                    />
                    <Divider />
                    <QueryPagination
                        query={transactionsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <TransactionModal
            id={id}
            onClose={() => {
                if(props.isAdmin == true) {
                    navigate(`/admin/transactions`);
                    return;
                }
                navigate(`/transactions`);
            }}
        />
    </>
}