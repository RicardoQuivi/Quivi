using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<ErrorType>))]
    public enum ErrorType
    {
        GenericError = 0,

        [JsonProperty("Não tem permissões para executar esta operação")]
        Unauthorized,

        #region Required

        [JsonProperty("^Não preencheu todos os campos obrigatórios.*")]
        MissingRequiredFields,

        [JsonProperty("^É obrigatória a indicação de um valor para o campo \\w+$")]
        MissingFieldValue,

        [JsonProperty("^Não preencheu .+ ou não existe .+ para utilizar$")]
        MissingFieldValueOrNotPreDefined,

        #endregion

        #region Not Exists

        [JsonProperty("O cliente não existe")]
        CustomerNotExists,

        [JsonProperty("O artigo não existe")]
        ItemNotExists,

        [JsonProperty("^(.+) referenciad(a|o) não existe$")]
        ReferenceEntityNotExists,

        [JsonProperty("^.* não existe$")]
        EntityXNotExists, // Make sure this enum value is the last of this. This will allow the serializer do select the most indicate one.

        #endregion

        [JsonProperty("^O valor indicado para o campo \\w+ já se encontra registado$")]
        FieldValueAlreadyExists,

        [JsonProperty("Já existe uma série com este descritivo")]
        SerieNameAlreadyExists,
        
        [JsonProperty("^O contribuinte introduzido aparenta ser inválido para \\w+$")]
        InvalidVatNumberForCountry,

        [JsonProperty("O tipo de documento não existe ou não se encontra activo")]
        InvalidOrInactiveDocumentType,

        [JsonProperty("Seleccione pelo menos um artigo")]
        SelectAtLeastOneItem,

        [JsonProperty("O estado do documento não é válido")]
        InvalidDocumentStatus,

        [JsonProperty("A série encontra-se expirada")]
        ExpiredSerie,

        [JsonProperty("^Um ou mais artigos mal preenchidos, na linha: \\d+$")]
        InvalidItemsAtLineX,

        [JsonProperty("Quando existe isenção de IVA é obrigatório que a taxa de IVA seja zero")]
        VatRateMustBe0WhenExemption,

        [JsonProperty("Quando o tipo de IVA seleccionado é «Não fazer nada» todos os artigos do documento devem ter a Taxa de IVA 0% e o respectivo motivo de isenção de IVA aplicado")]
        AllItemsVatRateMustBe0WhenIsDoNothingType,

        [JsonProperty("O desconto financeiro não pode ser superior à soma dos preços unitários dos artigos")]
        DiscountMustBeLessOrEqualThanTotalPrice,

        [JsonProperty("O país introduzido é inválido")]
        InvalidCountry,

        [JsonProperty("Configure primeiro o utilizador de acesso de webservice AT")]
        MissingATConfigurations,

        [JsonProperty("Todos os tipos de documento já se encontram comunicados!")]
        SerieAlreadyCommunicated,

        [JsonProperty("A série não se encontra comunicada no tipo de documento introduzido")]
        MissingSerieCommunicationForDocumentType,

        [JsonProperty("A série não se encontra comunicada à AT no tipo de documento seleccionado. A partir de 2023 passou a ser um requisito obrigatório")]
        MissingSerieATComunication,
    }
}
