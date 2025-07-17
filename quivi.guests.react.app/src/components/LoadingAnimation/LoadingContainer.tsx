import { LoadingAnimation } from "./LoadingAnimation"

export const LoadingContainer = () => {
    return <div style={{position: "relative", height: "100%"}}>
            <div className="animation__background" style={{height: "100%", width: "100%"}}>
                <LoadingAnimation />
            </div>
        </div>
}