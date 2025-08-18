using FacturaLusa.v2.Exceptions;
using System.Text.RegularExpressions;

namespace FacturaLusa.v2
{
    public static class ErrorTypeMapper
    {
        private static readonly List<(Regex Pattern, ErrorType Type)> _mappings = new()
        {
            (new Regex(@"^Não tem permissões para executar esta operação$"), ErrorType.Unauthorized),

            // Required
            (new Regex(@"^Não preencheu todos os campos obrigatórios.*"), ErrorType.MissingRequiredFields),
            (new Regex(@"^É obrigatória a indicação de um valor para o campo \w+$"), ErrorType.MissingFieldValue),
            (new Regex(@"^Não preencheu .+ ou não existe .+ para utilizar$"), ErrorType.MissingFieldValueOrNotPreDefined),

            // Not Exists
            (new Regex(@"^O cliente não existe$"), ErrorType.CustomerNotExists),
            (new Regex(@"^O artigo não existe$"), ErrorType.ItemNotExists),
            (new Regex(@"^(.+) referenciad(a|o) não existe$"), ErrorType.ReferenceEntityNotExists),
            (new Regex(@"^.* não existe$"), ErrorType.EntityXNotExists), // Should be last in this group

            // Others
            (new Regex(@"^O valor indicado para o campo \w+ já se encontra registado$"), ErrorType.FieldValueAlreadyExists),
            (new Regex(@"^Já existe uma série com este descritivo$"), ErrorType.SerieNameAlreadyExists),
            (new Regex(@"^O contribuinte introduzido aparenta ser inválido para \w+$"), ErrorType.InvalidVatNumberForCountry),
            (new Regex(@"^O tipo de documento não existe ou não se encontra activo$"), ErrorType.InvalidOrInactiveDocumentType),
            (new Regex(@"^Seleccione pelo menos um artigo$"), ErrorType.SelectAtLeastOneItem),
            (new Regex(@"^O estado do documento não é válido$"), ErrorType.InvalidDocumentStatus),
            (new Regex(@"^A série encontra-se expirada$"), ErrorType.ExpiredSerie),
            (new Regex(@"^Um ou mais artigos mal preenchidos, na linha: \d+$"), ErrorType.InvalidItemsAtLineX),
            (new Regex(@"^Quando existe isenção de IVA é obrigatório que a taxa de IVA seja zero$"), ErrorType.VatRateMustBe0WhenExemption),
            (new Regex(@"^Quando o tipo de IVA seleccionado é «Não fazer nada» todos os artigos do documento devem ter a Taxa de IVA 0% e o respectivo motivo de isenção de IVA aplicado$"), ErrorType.AllItemsVatRateMustBe0WhenIsDoNothingType),
            (new Regex(@"^O desconto financeiro não pode ser superior à soma dos preços unitários dos artigos$"), ErrorType.DiscountMustBeLessOrEqualThanTotalPrice),
            (new Regex(@"^O país introduzido é inválido$"), ErrorType.InvalidCountry),
            (new Regex(@"^Configure primeiro o utilizador de acesso de webservice AT$"), ErrorType.MissingATConfigurations),
            (new Regex(@"^Todos os tipos de documento já se encontram comunicados!$"), ErrorType.SerieAlreadyCommunicated),
            (new Regex(@"^A série não se encontra comunicada no tipo de documento introduzido$"), ErrorType.MissingSerieCommunicationForDocumentType),
            (new Regex(@"^A série não se encontra comunicada à AT no tipo de documento seleccionado. A partir de 2023 passou a ser um requisito obrigatório$"), ErrorType.MissingSerieATComunication),
        };

        public static ErrorType FromMessage(string message)
        {
            foreach (var (pattern, type) in _mappings)
                if (pattern.IsMatch(message))
                    return type;

            return ErrorType.GenericError;
        }
    }
}