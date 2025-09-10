interface Props {
    readonly message?: React.ReactNode | string;
}
export const InputErrorMessage = (props: Props) => {
    if(props.message == undefined) {
        return undefined;
    }

    if(typeof props.message === 'string') {
        return (
        <p
            className='mt-1.5 text-xs text-error-500'
        >
            {props.message}
        </p>
        )
    }

    return <div
        className='mt-1.5 text-xs text-error-500'
    >
        {props.message}
    </div>
}