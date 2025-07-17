interface Props {
    readonly name: string;
    readonly url: string;
    readonly logoUrl: string;
}

export const ExternalLink: React.FC<Props> = ({ name, url, logoUrl }) => {
    return (
        <a href={url} className="external-links__cell" target="_blank">
            <img src={logoUrl} alt={name} />
        </a>
    );
}