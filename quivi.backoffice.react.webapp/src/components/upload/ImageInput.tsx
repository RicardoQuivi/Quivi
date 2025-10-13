import { useEffect, useState } from 'react';
import Cropper, { Area, Point } from 'react-easy-crop';
import { useTranslation } from 'react-i18next';
import { useFileStorageApi } from '../../hooks/api/useFileStorageApi';
import { CheckLineIcon, CloseLineIcon, PencilIcon, TrashBinIcon, UndoIcon, UploadIcon } from '../../icons';
import React from 'react';
import { FileExtension } from '../../hooks/api/Dtos/fileStorage/FileExtension';
import { Tooltip } from '../ui/tooltip/Tooltip';
import { IconButton } from '../ui/button/IconButton';
import { Modal, ModalSize } from '../ui/modal';
import { ModalButtonsFooter } from '../ui/modal/ModalButtonsFooter';
import { FileDropZone } from './FileDropZone';
import { UploadHandler } from './UploadHandler';
import Label from '../form/Label';
import { Spinner } from '../spinners/Spinner';
import { Skeleton } from '../ui/skeleton/Skeleton';

const readFile = (file: File): Promise<string> => new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.addEventListener('load', () => {
        if(reader.result == null) {
            reject(null);
            return;
        }

        if(typeof reader.result === 'string' || reader.result instanceof String) {
            resolve(reader.result as string);
            return;
        }

        reject(null);
    }, false)
    reader.readAsDataURL(file)
})

const createImage = (url: string): Promise<HTMLImageElement> => new Promise((resolve, reject) => {
    const image = new Image()
    image.crossOrigin = 'anonymous'; // Enable CORS if needed
    image.addEventListener('load', () => resolve(image))
    image.addEventListener('error', error => reject(error))
    image.src = url;
})

const toBlob = async (imageSrc: string, pixelCrop?: Area): Promise<Blob> => {
    const image = await createImage(imageSrc);
    const canvas = document.createElement('canvas')

    const minPixels = 1000;
    const scaleW = minPixels / image.width;
    const scaleH = minPixels / image.height;
    const scale = scaleH * image.width < minPixels ? scaleW : scaleH;

    if(pixelCrop == undefined) {
        pixelCrop = {
            x: 0,
            y: 0,
            height: image.height,
            width: image.width,
        }
    }
    canvas.width = pixelCrop.width * scale;
    canvas.height = pixelCrop.height * scale;

    const ctx = canvas.getContext('2d');
    if(ctx == null) {
        throw new Error();
    }

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(
        image,
        pixelCrop.x,
        pixelCrop.y,
        pixelCrop.width,
        pixelCrop.height,
        0,
        0,
        canvas.width,
        canvas.height,
    );

    const file = await new Promise<Blob>((resolve) => canvas.toBlob(f => f != null && resolve(f), 'image/png'));
    return file;
}

interface Props {
    readonly label?: string;
    readonly value: string;
    readonly defaultValue?: string;
    readonly aspectRatio: number;
    readonly inlineEditor?: boolean;
    readonly onUploadHandlerChanged: (saveFunction: undefined | UploadHandler<string>) => void;
    readonly isLoading?: boolean;
}

interface ImageInputState {
    readonly edit: { readonly isOpen: boolean, readonly imageSrc: string }
    readonly image: { readonly url: string, readonly name: string }
}
const minZoom = 0.01;
export const ImageInput: React.FC<Props> = ({
    label,
    value,
    defaultValue,
    aspectRatio,
    inlineEditor,
    onUploadHandlerChanged,
    isLoading,
}) => {
    const  { t } = useTranslation();
    const fileApi = useFileStorageApi();

    const [cropResult, setCropResult] = useState({
        image: value as string | undefined,
        area: undefined as Area | undefined,
    })
    const [state, setState] = useState<ImageInputState>({
        edit: {
            isOpen: false,
            imageSrc: "",
        },
        image: {
            name: value,
            url: value,
        },
    })
    
    useEffect(() => setState({
        edit: {
            isOpen: false,
            imageSrc: "",
        },
        image: {
            name: value,
            url: value,
        },
    }), [value])

    useEffect(() => setCropResult({
        area: undefined,
        image: value,
    }), [value]);

    const onImageChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files.length > 0) {
            const file = e.target.files[0]
            await onFileChanged(file);
        }
    }

    const onFileChanged = async (file: File) => {
        const imageDataUrl = await readFile(file);
        setState(s => ({
            ...s,
            edit: {
                isOpen: true,
                imageSrc: imageDataUrl,
            },
            image: {
                ...s.image,
                name: file.name,
            },
        }))
    }

    const save = (blob: Blob, name: string) => new Promise<string>((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = async (readerEvt) => {
            try{
                if(readerEvt.target == null) {
                    reject(null);
                    return;
                }

                const base64 = readerEvt.target.result as string;

                const aux = ";base64,";
                const index = base64.indexOf(aux);
                
                const response = await fileApi.uploadImage({
                    base64Data: base64.substring(index + aux.length),
                    extension: FileExtension.PNG,
                    name: name,
                })

                resolve(response.data);
            } catch (e) {
                reject(e);
            }
        };
        reader.readAsDataURL(blob);
    })

    const dropZoneActive = !state.image.url && !(inlineEditor == true && state.edit.isOpen);
    return <div className='grid grid-cols-1'>
        {
            label != undefined &&
            <Label>{label}</Label>
        }
        <div className="relative">
            <div
                className={isLoading == true ? "invisible" : ""}
            >
                {
                    isLoading != true &&
                    !dropZoneActive &&
                    <div 
                        className="absolute top-0 bottom-0 left-0 right-0 flex flex-col"
                        style={inlineEditor == true && state.edit.isOpen == false ? {
                            backgroundColor: "white",
                            backgroundImage: "linear-gradient(45deg, #ccc 25%, transparent 25%), linear-gradient(-45deg, #ccc 25%, transparent 25%), linear-gradient(45deg, transparent 75%, #ccc 75%), linear-gradient(-45deg, transparent 75%, #ccc 75%)",
                            backgroundSize: "40px 40px",
                            backgroundPosition: "0 0, 0 20px, 20px -20px, -20px 0px"
                        } : {}}
                    >
                        <div className={`flex flex-col items-center absolute top-0 bottom-0 size-full ${inlineEditor == true && state.edit.isOpen ? "visible" : "invisible"}`}>
                            <div className="flex-auto w-full">
                                <CropImage
                                    imageSrc={state.edit.imageSrc}
                                    aspectRatio={aspectRatio}
                                    onChange={(image, area) => setCropResult({
                                        area: area,
                                        image: image,
                                    })}
                                    initialCroppedAreaPixels={cropResult.area}
                                    hideZoomSlider
                                />
                            </div>

                            <div className="flex items-center gap-3 px-2 mt-6 lg:justify-end w-full">
                                <IconButton
                                    className="w-full"
                                    onClick={() => {
                                        onUploadHandlerChanged(undefined);
                                        setState(s => ({ 
                                            ...s,
                                            edit: {
                                                isOpen: false,
                                                imageSrc: s.image.url,
                                            },
                                        }));
                                    }}
                                >
                                    <CloseLineIcon className="fill-error-500 dark:fill-error-500 h-6 w-6" />
                                </IconButton>
                                <IconButton
                                    className="w-full"
                                    onClick={async () => {
                                        if(cropResult.image == undefined) {
                                            return;
                                        }

                                        const blob = await toBlob(cropResult.image, cropResult.area);
                                        const saveFunction = () => save(blob, state.image.name);
                                        onUploadHandlerChanged(new UploadHandler(saveFunction));
                                        setState(s => ({
                                            ...s,
                                            image: {
                                                ...s.image,
                                                url: URL.createObjectURL(blob),
                                            },
                                            edit: {
                                                ...s.edit,
                                                isOpen: false,
                                            },
                                        }));
                                    }}
                                >
                                    <CheckLineIcon className="h-6 w-6" />
                                </IconButton>
                            </div>
                        </div>
                        
                        <div
                            className={`size-full bg-no-repeat bg-center bg-contain ${inlineEditor == true && state.edit.isOpen ? "invisible" : "visible"}`}
                            style={{
                                backgroundImage: `url("${state.image.url ?? defaultValue ?? "/Images/image-placeholder.svg"}")`,
                            }}
                        >
                            <div className="flex items-center gap-3 py-2 px-2 mt-6 lg:justify-end w-full bottom-0 left-0 right-0 absolute justify-end bg-gray-900/60 dark:bg-gray-100/60">
                                {
                                    !!state.edit.imageSrc &&
                                    <Tooltip message={t("imageEditor.edit")}>
                                        <IconButton
                                            onClick={() => setState(s => ({ 
                                                ...s,
                                                edit: {
                                                    ...s.edit,
                                                    isOpen: true,
                                                    imageSrc: s.edit.imageSrc,
                                                },
                                            }))}
                                            className='dark:text-gray-900 text-white'
                                        >
                                            <PencilIcon className="h-6 w-6" />
                                        </IconButton>
                                    </Tooltip>
                                }
                                <Tooltip message={t("imageEditor.upload")}>
                                    <label
                                        className='"fill-white dark:fill-gray-800 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm w-8 h-8 inline-flex justify-center items-center dark:hover:bg-gray-600 dark:hover:text-white dark:text-gray-900 text-white cursor-pointer'
                                    >
                                        <input
                                            type="file"
                                            accept="image/jpeg,image/gif,image/png"
                                            onChange={onImageChange}
                                            className="hidden"
                                        />
                                        <UploadIcon className="h-6 w-6" />
                                    </label>
                                </Tooltip>

                                {
                                    state.image.url != value &&
                                    <Tooltip message={t("imageEditor.undo")}>
                                        <IconButton
                                            onClick={() => {
                                                onUploadHandlerChanged(undefined);
                                                setState(s => ({ ...s, image: { ...s.image, url: value, }, edit: { ...s.edit, imageSrc: "" } }));
                                            }}
                                            className='dark:text-gray-900 text-white'
                                        >
                                            <UndoIcon className="h-6 w-6" />
                                        </IconButton>
                                    </Tooltip>
                                }
                                {
                                    !!state.image.url &&
                                    <Tooltip message={t("imageEditor.delete")}>
                                        <IconButton
                                            onClick={() => {
                                                const image = "";
                                                setState(s => ({
                                                    ...s,
                                                    image: {
                                                        name: image,
                                                        url: image,
                                                    }
                                                }))
                                                const saveFunction = !value ? undefined : async () => image;
                                                onUploadHandlerChanged(saveFunction == undefined ? undefined : new UploadHandler(saveFunction));
                                            }}
                                            className='dark:text-gray-900 text-white'
                                        >
                                            <TrashBinIcon className="h-6 w-6"/>
                                        </IconButton>
                                    </Tooltip>
                                }
                            </div>
                        </div>
                    </div>
                }
                <div className={`size-full relative flex flex-col justify-center ${isLoading ? "invisible" : dropZoneActive ? "visible" : "collapse"}`}>
                    <FileDropZone
                        allowedFiles={[
                            FileExtension.JPEG,
                            FileExtension.JPG,
                            FileExtension.PNG,
                        ]}
                        onFilesDroped={(files) => onFileChanged(files[0])}
                    />
                </div>
            </div>
            {
                isLoading == true &&
                <Skeleton className="absolute inset-0"/>
            }
        </div>
        {
            inlineEditor != true &&
            <EditImageModal 
                isOpen={state.edit.isOpen}
                imageSrc={state.edit.imageSrc}
                aspectRatio={aspectRatio}
                onClose={() => setState(s => ({ 
                    ...s,
                    edit: {
                        isOpen: false,
                        imageSrc: s.image.url,
                    },
                }))}
                onChange={async (image, area) => {
                    const blob = await toBlob(image, area);                
                    const saveFunction = () => save(blob, state.image.name);
                    onUploadHandlerChanged(new UploadHandler(saveFunction));
                    setState(s => ({
                        ...s,
                        image: {
                            ...s.image,
                            url: URL.createObjectURL(blob),
                        },
                        edit: {
                            ...s.edit,
                            isOpen: false,
                        },
                    }));
                }}
            />
        }
    </div>
}

interface EditImageModal {
    readonly isOpen: boolean;
    readonly imageSrc: string;
    readonly aspectRatio: number;
    readonly onClose: () => any;
    readonly onChange: (image: string, area: Area) => Promise<any>;
}
const EditImageModal = (props: EditImageModal) => {
    const { t } = useTranslation();

    const [isLoading, setIsLoading] = useState(false);
    const [state, setState] = useState({
        image: props.imageSrc as string | undefined,
        area: {
            height: 0,
            width: 0,
            x: 0,
            y: 0,
        }
    })

    return <Modal
        isOpen={props.isOpen}
        title={t("imageEditor.Edit")}
        size={ModalSize.Large}
        footer={
            <ModalButtonsFooter 
                primaryButton={{
                    content: isLoading
                                ?
                                <Spinner />
                                :
                                t("common.confirm"),
                    disabled: isLoading,
                    onClick: async () => {
                                if(state.image == undefined) {
                                    return;
                                }
    
                                setIsLoading(true);
                                await props.onChange(state.image, state.area);
                                setIsLoading(false);
                            },
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}              
            />
        }
        onClose={props.onClose}
    >
        <div
            style={{
                height: "70dvh",
            }}
        >
            <CropImage 
                imageSrc={props.imageSrc}
                aspectRatio={props.aspectRatio} 
                onChange={(image, area) => image != undefined && area != undefined && setState({
                    area: area,
                    image: image,
                })}
            />
        </div>
    </Modal>
}

interface CropImageState {
    readonly initialArea: Area | undefined;
    readonly zoom: number;
    readonly crop: Point;
    readonly imageUrl: string;
}
interface CropImageProps {
    readonly imageSrc: string;
    readonly aspectRatio: number;
    readonly initialCroppedAreaPixels?: Area | undefined;
    readonly onChange: (imageSrc: string | undefined, area: Area | undefined) => any;
    readonly hideZoomSlider?: boolean;
}
const CropImage = (props: CropImageProps) => {
    const { t } = useTranslation();

    const [statesMap, setStatesMap] = useState<Map<string, CropImageState>>(new Map<string, CropImageState>());
    
    useEffect(() => {
        if(!props.imageSrc) {
            props.onChange(undefined, undefined);
            return;
        }

        toBlob(props.imageSrc).then(r => {
            const url = URL.createObjectURL(r);            
            setStatesMap(m => {
                const result = new Map<string, CropImageState>(m);
                result.set(props.imageSrc, {
                    initialArea: props.initialCroppedAreaPixels,
                    crop: {
                        x: 0,
                        y: 0
                    },
                    imageUrl: url,
                    zoom: 1,
                })
                return result;
            });
        });
    }, [props.imageSrc])
    
    return (
        <div className="flex flex-col items-center gap-3 px-2 mt-6 lg:justify-end size-full">
        {
            [props.imageSrc].filter(i => !!i).map(src => {
                const state = statesMap.get(src);
                if(state == undefined) {
                    return <React.Fragment key={src}/>
                }

                return <React.Fragment key={src}>
                    <div className='relative w-full flex-auto bg-white'>
                        <Cropper
                            key={src}
                            classes={{
                                containerClassName: "size-full"
                            }}
                            style={{
                                containerStyle: {
                                    backgroundImage: `linear-gradient(45deg, #e0e0e0 25%, transparent 25%, transparent 75%, #e0e0e0 75%, #e0e0e0), linear-gradient(45deg, #e0e0e0 25%, transparent 25%, transparent 75%, #e0e0e0 75%, #e0e0e0)`,
                                    backgroundSize: '20px 20px',
                                    backgroundPosition: '0 0, 10px 10px',
                                },
                            }}
                            image={state?.imageUrl}

                            restrictPosition={false}
                            initialCroppedAreaPixels={state.initialArea}
                            initialCroppedAreaPercentages={state.initialArea == undefined ? {
                                height: 100 * props.aspectRatio,
                                width: 100 / props.aspectRatio,
                                x: 0,
                                y: 0
                            } : undefined}
                            aspect={props.aspectRatio}

                            crop={state.crop}
                            onCropChange={c => setStatesMap(m => {
                                const result = new Map<string, CropImageState>(m);
                                const s = result.get(src)!;
                                result.set(src, {
                                    ...s,
                                    crop: c,
                                })
                                return result;
                            })}

                            zoom={state.zoom}
                            onZoomChange={zoom => setStatesMap(m => {
                                const result = new Map<string, CropImageState>(m);
                                result.set(src, {
                                    ...state,
                                    zoom: zoom,
                                })
                                return result;
                            })}
                            
                            minZoom={minZoom}
                            zoomSpeed={0.05}

                            objectFit='contain'
                            onCropComplete={(_: Area, croppedAreaPixels: Area) => props.onChange(state.imageUrl, croppedAreaPixels)}
                        />
                    </div>
                    {
                        props.hideZoomSlider != true &&
                        <div key={src} className="flex items-center">
                            <p>{t("imageEditor.zoom")}</p>
                            <input 
                                type="range"
                                className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer dark:bg-gray-700 flex-auto ml-1 px-0 py-2"
                                value={state.zoom}
                                min={minZoom}
                                max={3}
                                step={0.01}
                                onChange={(e) => setStatesMap(m => {
                                    const result = new Map<string, CropImageState>(m);
                                    result.set(src, {
                                        ...state,
                                        zoom: +e.target.value,
                                    })
                                    return result;
                                })}
                            />
                        </div>
                    }
                </React.Fragment>
            })
        }
        </div>
    )
}