
//TODO: Implement Scan Qr Code
export const ScanQrCode = () => {
    return <></>
    // const { t } = useTranslation();
    // const [readerIsOpen, setReaderIsOpen] = useState(false);

    // const openReader = () => setReaderIsOpen(true);
    // const closeReader = () => setReaderIsOpen(false);
    // const handleQRCodeResult = async (result: any, error: any) => {
    //     if (result) {
    //         window.location.href = result.text;
    //     } 
    // };

    // return (
    //     <motion.div initial='initial' animate='in' exit='out' transition={pageTransition} variants={pageVariants}>
    //         <MainNav />
    //         <section className="user">
    //             <div className="user__header">
    //                 <div className="container">
    //                     <h1>{t("scanQrCode.title")}</h1>
    //                 </div>
    //             </div>
    //             <div className="container">
    //                 <div className="scan__tutorial">
    //                     <div className="step">
    //                         <div className="step__number">
    //                             <span>1</span>
    //                         </div>
    //                         <p>{t("scanQrCode.step1")}</p>
    //                     </div>
    //                     <div className="step">
    //                         <div className="step__number">
    //                             <span>2</span>
    //                         </div>
    //                         <p>{t(`scanQrCode.step2`)}</p>
    //                     </div>
    //                     <div className="step">
    //                         <div className="step__number">
    //                             <span>3</span>
    //                         </div>
    //                         <p>{t("scanQrCode.step3")}</p>
    //                     </div>
    //                 </div>
    //                 <ButtonsSection>
    //                     <button type="button" className="primary-button" onClick={openReader}>{t("scanQrCode.openCamera")}</button>
    //                     {undefined}
    //                 </ButtonsSection>
    //             </div>

    //             <div className={`reader ${readerIsOpen ? "open" : ""}`}>
    //                 <div className="reader__background" onClick={closeReader}></div>
    //                 <div className="nav">
    //                     <div className="container flex flex-jc-e">
    //                         <button type="button" className="nav__close" onClick={closeReader}>
    //                             <CloseIcon />
    //                         </button>
    //                     </div>
    //                 </div>
    //                 <QrReader
    //                     constraints={{ facingMode: "environment" }}
    //                     containerStyle={{ width: "100%" }}
    //                     onResult={handleQRCodeResult}
    //                 />
    //             </div>

    //         </section>
    //     </motion.div>
    // );
};
