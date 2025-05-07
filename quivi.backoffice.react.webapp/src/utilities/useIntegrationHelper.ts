import { useMemo } from "react";
import { IntegrationType } from "../hooks/api/Dtos/posIntegrations/PosIntegration";

const getStrategyName = (type: IntegrationType): string => {
    switch(type)
    {
        case IntegrationType.QuiviViaFacturalusa: return "Quivi via FacturaLusa";
    }
}

export const useIntegrationHelper = () => {

    const result = useMemo(() => ({
        getStrategyName
    }), [])

    return result;
}