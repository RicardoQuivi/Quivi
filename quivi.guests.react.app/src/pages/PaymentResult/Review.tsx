import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuiviTheme } from '../../hooks/theme/useQuiviTheme';
import { Alert, Rating } from '@mui/material';
import { StarIcon, StarOutlineIcon } from '../../icons';
import { useReviewMutator } from '../../hooks/mutators/useReviewMutator';
import LoadingButton from '../../components/Buttons/LoadingButton';

interface Props {
    readonly transactionId: string,
}
export const Review = (props: Props) => {
    const { t } = useTranslation();
    const theme = useQuiviTheme();

    const [state, setState] = useState({
        isLoading: false,
        stars: 3,
        textReview: "",
        textReviewIsTouched: false,
        reviewError: false,
    })
    const mutator = useReviewMutator();
    
    const setIsLoading = (l: boolean) => setState(s => ({...s, isLoading: l}));
    const setRating = (l: number) => setState(s => ({...s, stars: l}));
    const setTextReview = (l: string) => setState(s => ({...s, textReview: l}));
    const setTextReviewIsTouched = (l: boolean) => setState(s => ({...s, textReviewIsTouched: l}));

    const reviewSubmitHandler = async (event: React.FormEvent) => {
        event.preventDefault();
        setIsLoading(true);

        await mutator.update({
            transactionId: props.transactionId,
            comment: state.textReview,
            stars: state.stars,
        })
        setIsLoading(false);
    }

    return (
        <div className="review">
            <form onSubmit={reviewSubmitHandler}>
                <div className="review__stars">
                    <Rating
                        value={state.stars}
                        precision={1}
                        onChange={(_, newValue) => {
                            if(newValue == null) {
                                return;
                            }
                            setRating(newValue);
                        }}
                        size='large'
                        emptyIcon={<StarOutlineIcon style={{ opacity: 0.55, width: "2.5rem", height: "auto" }} />}
                        icon={<StarIcon style={{ width: "2.5rem", height: "auto", fill: theme.primaryColor.hex }} />}
                    />
                </div>
                <div className="review__text">
                    <textarea
                        placeholder={`${t("review.textPlaceholder")} ${state.stars >= 4 || state.stars === 0 ? t("review.optional") : ""}`}
                        onChange={e => setTextReview(e.currentTarget.value)}
                        value={state.textReview}
                        onBlur={() => setTextReviewIsTouched(true)}
                    />
                    {
                        state.textReviewIsTouched && state.stars < 4 && state.textReview.length === 0 &&
                        <Alert variant="outlined" severity="warning">
                            {t("form.requiredField")}
                        </Alert>
                    }
                </div>
                <LoadingButton
                    isLoading={state.isLoading}
                    type="submit"
                    className="mt-6"
                    disabled={state.stars < 4 && state.textReview.length == 0}
                >
                    {t("review.send")}
                </LoadingButton>
                {
                    state.reviewError &&
                    <Alert variant="outlined" severity="warning">
                        {t("review.reviewErrorMessage")}
                    </Alert>
                }
            </form>
        </div>
    );
}