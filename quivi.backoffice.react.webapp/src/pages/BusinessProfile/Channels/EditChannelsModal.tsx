import { useTranslation } from "react-i18next";
import { useChannelMutator } from "../../../hooks/mutators/useChannelMutator";
import { ChannelProfile } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { useEffect, useMemo, useState } from "react";
import { useChannelsQuery } from "../../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { Modal, ModalSize } from "../../../components/ui/modal";
import { ModalButtonsFooter } from "../../../components/ui/modal/ModalButtonsFooter";
import Alert from "../../../components/ui/alert/Alert";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { SingleSelect } from "../../../components/inputs/SingleSelect";
import { TextField } from "../../../components/inputs/TextField";

interface Props {
    readonly applyToAll?: boolean;
    readonly channelIds: string[];
    readonly isOpen: boolean;
    readonly channelProfileId?: string;
    readonly onClose: () => void,
}
export const EditChannelsModal = (props: Props) => {
    const { t } = useTranslation();
    const mutator = useChannelMutator();

    const [state, setState] = useState({
        name: "",
        channelProfile: undefined as (undefined | ChannelProfile),
        isSubmiting: false,
    })

    const isMultiEdition = props.channelIds.length > 1 || props.applyToAll;
    const channelsQuery = useChannelsQuery(props.isOpen == false ? undefined : {
        ids: props.channelIds,
        page: 0,
        pageSize: props.channelIds.length,
    })

    const profilesQuery = useChannelProfilesQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: undefined,
    });

    const {
        profilesMap,
        defaultProfile,
    } = useMemo(() => {
        const map = new Map<string, ChannelProfile>();
        for(const p of profilesQuery.data) {
            map.set(p.id, p);
        }

        const firstProfile = profilesQuery.data.length == 0 ? undefined : profilesQuery.data[0];
        return {
            profilesMap: map,
            defaultProfile: props.channelProfileId == undefined ? undefined : map.get(props.channelProfileId) ?? firstProfile,
        }
    }, [profilesQuery.data])
    
    useEffect(() => {
        if(channelsQuery.isFirstLoading || profilesQuery.isFirstLoading) {
            return;
        }

        if(channelsQuery.data.length != 1) {
            return;
        }

        const channel = channelsQuery.data[0];
        setState(s => ({
            ...s,
            name: channel.name, 
            channelProfile: channel.channelProfileId == undefined ? undefined : profilesMap.get(channel.channelProfileId),
        }))
    }, [
        channelsQuery.isFirstLoading, channelsQuery.data,
        profilesMap,
    ])

    useEffect(() => {
        if(state.channelProfile != undefined) {
            return;
        }

        setState(s => ({ ...s, channelProfile: defaultProfile }));
    }, [defaultProfile])

    const onSubmit = async () => {
        setState(s => ({...s, isSubmiting: true}))
        try {
            await mutator.patch(channelsQuery.data, {
                name: state.name,
                channelProfileId: state.channelProfile?.id,
            });
            props.onClose();
        } finally {
            setState(s => ({...s, isSubmiting: false}))
        }
    }

    return <Modal
        isOpen={props.isOpen}
        onClose={props.onClose}
        size={ModalSize.Medium}
        title={t('common.operations.edit', {
            name: t(`common.entities.${props.channelIds.length == 1 ? "channel" : "channels"}`),
        })}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: t("common.confirm"),
                    onClick: onSubmit,
                    disabled: profilesQuery.isFirstLoading,
                    isLoading: state.isSubmiting,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        {
            isMultiEdition &&
            <Alert
                variant="info"
                message={t("pages.channels.editMultipleWarning")}
                showLink={false}
            />
        }
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-1 lg:grid-cols-1 gap-4">
            {
                isMultiEdition == false &&
                <TextField
                    label={t("common.name")}
                    type="text"
                    className="w-full"
                    value={state.name}
                    onChange={e => setState(s => ({...s, name: e}))}
                />
            }
            <div>
                {
                    profilesQuery.isFirstLoading
                    ?
                    <Skeleton className="w-full h-full"/>
                    :
                    <SingleSelect
                        options={profilesQuery.data}
                        value={state.channelProfile}
                        getId={e => e?.id ?? ""}
                        onChange={(e) => setState(s => ({...s, channelProfile: e}))}
                        render={e => e?.name ?? ""}
                        label={t(`common.entities.channelProfile`)}
                    />
                }
            </div>
        </div>
    </Modal>
}