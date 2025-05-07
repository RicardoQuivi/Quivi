import { JSX } from 'react';
import { useTranslation } from "react-i18next";

interface Props {
    readonly currentPage: number;
    readonly numberOfPages: number;
    readonly onPageChanged: (toPage: number) => void;
}

export const PaginationFooter: React.FC<Props> = ({
    currentPage,
    numberOfPages,
    onPageChanged,
}) => {
    const { t } = useTranslation();

    const getLi = (key: React.Key, text: any, isActive: boolean, onClick?: () => any) => (<li key={key} className={`page-item ${isActive ? "active" : ""}`}>
        <span className="page-link" style={onClick == undefined ? undefined : {cursor: "pointer"}} onClick={onClick}>{text}</span>
    </li>)

    const generatePages = (): JSX.Element[] => {
        const result: JSX.Element[] = [ getLi(currentPage, currentPage, true)];

        const maxDistance = 2;
        for(let i = 0; i < maxDistance; ++i) {
            let previousPage = currentPage - i;
            if(1 <= previousPage) {
                result.unshift(getLi(previousPage, previousPage, false, () => onPageChanged(previousPage)))
            }

            let nextPage = currentPage + i;
            if(nextPage <= numberOfPages) {
                result.push(getLi(nextPage, nextPage, false, () => onPageChanged(nextPage)))
            }
        }

        const nextDistance = maxDistance + 1;
        if(1 == currentPage - nextDistance) {
            result.unshift(getLi(1, 1, false, () => onPageChanged(0)));
        } else if(1 < currentPage - nextDistance) {
            result.unshift(getLi('...-previous', '...', false));
            result.unshift(getLi(1, 1, false, () => onPageChanged(0)));
        }

        if(currentPage + nextDistance == numberOfPages) {
            result.push(getLi(numberOfPages, numberOfPages, false, () => onPageChanged(numberOfPages - 1)));
        } else if(currentPage + nextDistance < numberOfPages){
            result.push(getLi('...-next', '...', false));
            result.push(getLi(numberOfPages, numberOfPages, false, () => onPageChanged(numberOfPages - 1)));
        }
        return result;
    }

    return (
        <>
        {
            numberOfPages > 1 &&
            <nav style={{ display: "flex", width: "100%", alignItems: "center", flexDirection: "column" }}>
                <ul className="pagination" style={{margin: "0.5rem 0"}}>
                    {
                        numberOfPages > 1 && currentPage > 0 &&
                        <li className="page-item previous">
                            <span className="page-link link" style={{cursor: "pointer"}} onClick={() => onPageChanged(currentPage - 1)} >{t("previous")}</span>
                        </li>
                    }

                    {generatePages()}

                    {
                        currentPage < numberOfPages &&
                        <li className="page-item next">
                            <span className="page-link link" style={{cursor: "pointer"}} onClick={() => onPageChanged(currentPage + 1)}>{t("next")}</span>
                        </li>
                    }
                </ul>
            </nav>
        }
        </>
    );
}