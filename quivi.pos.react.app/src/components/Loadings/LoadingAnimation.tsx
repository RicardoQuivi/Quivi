export const LoadingAnimation = () => {
    return (
        <div className="container" style={{display: "flex", flexDirection: "column", alignItems: "center", margin: "3rem 0" }}>
            <div className="spinner-border" role="status" style={{width: "2rem", height: "2rem"}}>
                <span className="sr-only">Loading...</span>
            </div>
        </div>
    );
};