import { HelmetProvider, Helmet } from "react-helmet-async";

interface PageMetaProps {
    readonly title: string;
    readonly description: string;
}
const PageMeta = (props: PageMetaProps) => (
    <Helmet>
        <title>{props.title}</title>
        <meta name="description" content={props.description} />
    </Helmet>
);

export const AppWrapper = ({ children }: { children: React.ReactNode }) => (
    <HelmetProvider>{children}</HelmetProvider>
);
export default PageMeta;