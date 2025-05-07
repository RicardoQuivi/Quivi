import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta"
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useAuth } from "../../../context/AuthContext";
import { useMerchantsQuery } from "../../../hooks/queries/implementations/useMerchantsQuery";
import { useNavigate } from "react-router";
import { useMemo, useState } from "react";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import Button from "../../../components/ui/button/Button";
import { ClipLoader } from "react-spinners";
import { useMerchantMutator } from "../../../hooks/mutators/useMerchantMutator";

export const TermsAndConditionsPage = () => {
    const { t } = useTranslation();
    const auth = useAuth();
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const mutator = useMerchantMutator();

    if(auth.merchantId == undefined) {
        navigate("/");
        return;
    }

    const merchantsQuery = useMerchantsQuery({
        id: auth.merchantId,
        page: 0,
        pageSize: 1,
    })

    const subMerchantsQuery = useMerchantsQuery({
        parentId: auth.merchantId,
        page: 0,
    })
    
    const merchant = useMemo(() => {
        if(merchantsQuery.data.length == 0) {
            return undefined;
        }
        return merchantsQuery.data[0];
    }, [merchantsQuery.data])

    const save = async () => {
        if(merchant == undefined) {
            return;
        }

        setIsSubmitting(true);
        try {
            await mutator.patch(merchant, {
                acceptTermsAndConditions: true,
            });
            auth.switchMerchant(auth.subMerchantId ?? auth.merchantId ?? merchant.id);
            navigate("/");
        } finally {
            setIsSubmitting(false);
        }
    }

    return <>
        <PageMeta
            title={t("pages.termsAndConditions.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.termsAndConditions.title")} />

        <ComponentCard
            desc='Os presentes Termos e Condições destinam-se a regular o fornecimento da plataforma (doravante "QUIVI"), bem como das funcionalidades que lhe estão associadas.'
            className="mt-4"
        >
            <ul>
                {/* 1º Term */}
                <li>
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        1. A QUIVI é disponibilizada para uso da Entidade { merchant == undefined ? <Skeleton className="w-sm"/> : <b>{merchant.name}</b> } com o NIPC { merchant == undefined ? <Skeleton className="w-sm"/> : <b>{merchant.vatNumber}</b> } (doravante "CLIENTE ACEITANTE"),
                        e dos seus clientes finais (doravante "UTILIZADORES")
                        através da sociedade comercial QUIVI, LDA. com o NIPC 11111111111 (doravante QUIVI) sediada na Av. ABC 76, 0000-000 Lisboa.
                        O QUIVI é uma plataforma destinada a facilitar o relacionamento comercial do CLIENTE ACEITANTE com os UTILIZADORES, disponibilizando a estes as seguintes funcionalidades:
                    </p>
                    <ul className="ml-4">
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                1.1 Um portal disponível através do endereço https://portal.quivi.com, com as seguinte funcionalidades:
                            </p>
                            <ul>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.1.1 Registo e serviços de autenticação de conta para acesso às áreas reservadas.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.1.2 Gestão de perfil de comerciante, com recurso a elementos que permitem a criação de uma identidade comercial na plataforma QUIVI.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.1.3 Gestão de menu estático servido por ficheiro(s) PDF, ou menu interactivo digital com a possibilidade de gestão de pedidos.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.1.4 Criação e visualização de códigos QR.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.1.5 Aceitação dos instrumentos de pagamento MB WAY, Ticket Restaurant Mobile, VISA, Mastercard e/ou Apple Pay e Google Pay.
                                    </p>
                                </li>
                            </ul>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                1.2 Uma app para smartphone ou tablet denominada Quivi Négócios, com as seguintes funcionalidades:
                            </p>
                            <ul>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.2.1 Monitorização de pagamentos e gorjetas em tempo real.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.2.2 Visualização de histórico de pagamentos e gorjetas.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.2.3 Acesso à execução de devoluções, parciais ou totais.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.2.4 Acesso a avaliações registadas pelos utilizadores após cada pagamento.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.2.5 Acesso aos recebimentos totais diários, por meio de pagamento.
                                    </p>
                                </li>
                            </ul>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                1.3 A disponibilização de uma webapp - acessível através de um código QR ou link, destinada a ser utilizada pelo UTILIZADOR para:
                            </p>
                            <ul>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.3.1 Consulta de bens e serviços a pagamento.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.3.2 Gestão de perfil de utilizador, com recurso a elementos que permitem a criação de uma identidade na plataforma QUIVI.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.3.3 Registo de avaliação de experiência com recurso a escala de 1-5, e campo livre de comentário.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.3.4 Acesso às funcionalidades de pagamento total, parcial, ou através da selecção dos bens e serviços consumidos.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.3.5 Execução de pagamentos através de MB WAY, Ticket Restaurant Mobile, VISA, Mastercard e/ou Apple Pay e Google Pay.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        1.3.6 Acesso a um menu digital interactivo (quando configurado e disponibilizado pelo "CLIENTE ACEITANTE"), com a possibilidade de criação de pedidos.
                                    </p>
                                </li>
                            </ul>
                        </li>
                    </ul>
                </li>

                {/* 2º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        2. As obrigações do CLIENTE ACEITANTE incluem:
                    </p>
                    <ul className="ml-4">
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                a) Assegurar a aceitação, no estabelecimento, o pagamento de bens e serviços através do sistema QUIVI, de forma correcta através da verificação dos recebimentos;
                            </p>
                        </li>

                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                b) Aceitar o desconto da comissão de serviço acordada no contrato e incidente sobre o valor de cada transacção de bens ou serviços, cujo pagamento tenha sido efectuado mediante a utilização da QUIVI;
                            </p>
                        </li>

                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                c) Restituir imediatamente à QUIVI ou à instituição financeira que lhe serve de apoio, por débito da sua conta bancária ou por compensação que desde já autoriza, as importâncias que esta lhe tenha feito creditar nos casos em que tenha sido violada qualquer das cláusulas deste contrato, ou nos casos em que tenham sido liquidados, através da instituição financeira que presta apoio à QUIVI importâncias que não fossem devidas ou relativas a transacções que tenham sido anuladas, canceladas ou rectificadas.
                            </p>
                        </li>

                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                d) Impedir que terceiros tenham acesso a qualquer documentação ou equipamento, directa ou indirectamente relacionados com a QUIVI, responsabilizando-se perante a QUIVI por qualquer acto culposo que dai decorra.
                            </p>
                        </li>

                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                e) Facultar à QUIVI ou a entidade por esta indicada, em caso de reclamação ou de suspeita de fraude ou de violação do sistema QUIVI, o acesso a documentos e a informações complementares atinentes a toda e qualquer transacção.
                            </p>
                        </li>
                    </ul>
                </li>

                {/* 3º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        3. As obrigações da QUIVI incluem:
                    </p>
                    <ul className="ml-4">
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                Proceder, relativamente a cada transacção e através da instituição financeira que lhe serve de apoio, à liquidação financeira ao CLIENTE ACEITANTE do respectivo valor, já descontado da comissão de serviço devida pelo estabelecimento à QUIVI nas condições e prazos acordados, por meio de transferência a crédito da conta bancária, da titularidade do Estabelecimento cujo IBAN consta deste contrato.
                            </p>
                        </li>
                    </ul>
                </li>

                {/* 4º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        4. Nenhuma informação ou conteúdo da QUIVI devem ser interpretados como atribuindo qualquer licença ou autorização para a utilização de qualquer direito de propriedade intelectual ou afim nele divulgado ou contido.
                    </p>
                </li>

                {/* 5º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        5. São proibidos por lei todos os actos que, nos termos da legislação aplicável, possam constituir uma violação dos direitos de propriedade intelectual e afins de que a QUIVI e terceiros sejam titulares, devendo os utilizadores abster-se de os praticar.
                    </p>
                </li>

                {/* 6º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        6. A QUIVI não se responsabiliza por qualquer atraso, suspensão ou interrupção da QUIVI, resultante de circunstâncias alheias ao seu controlo.
                    </p>
                </li>

                {/* 7º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        7. A QUIVI e todas as informações e demais elementos constantes da plataforma podem ser alterados sem aviso prévio sem que por esse facto incorra a QUIVI em qualquer responsabilidade.
                    </p>
                </li>

                {/* 8º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        8. Salvo em caso de dolo ou culpa grave por parte da QUIVI, esta não se responsabilizará por qualquer dano ou prejuízo, independentemente da sua natureza, que o CLIENTE ACEITANTE venha a suportar em consequência directa ou indirecta:
                    </p>
                    <ul className="ml-4">
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (a) de qualquer inexactidão, erro, omissão, deficiência de dados ou informações facultados através da QUIVI;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (b) de atrasos ou interrupções no fornecimento de dados ou informações facultados através da QUIVI;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (c) de qualquer decisão ou acção tida pelo utilizador ou por terceiros com base na informação facultada através da QUIVI, ainda que esta seja inexacta ou incorrecta.
                            </p>
                        </li>
                    </ul>
                </li>

                {/* 9º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        9. A utilização da QUIVI implica o tratamento dos dados dos utilizadores pela QUIVI. As disposições aplicáveis em matéria de protecção de dados dos titulares, estão disponíveis na Política de Privacidade a qual deverá ser aceite para activação de conta.
                    </p>
                </li>

                {/* 10º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        10. A utilização da app Quivi Negócios realiza-se mediante a introdução de um código (ID) de ACTIVAÇÃO fornecido pelo sistema QUIVI.  O CLIENTE ACEITANTE é integralmente responsável pela confidencialidade e segurança do código (ID) de ACTIVAÇÃO.
                    </p>
                </li>

                {/* 11º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                    11. O CLIENTE ACEITANTE deverá utilizar a QUIVI garantindo que:
                    </p>
                    <ul className="ml-4">
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (a) não viole qualquer lei;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (b) não prejudique ou danifique o funcionamento da mesma, incluindo quaisquer redes informáticas a ele ligadas;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (c) não seja causador de qualquer risco de confusão com qualquer outra pessoa ou entidade;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (d) não carregue, disponibilize, transmita, publique ou distribua qualquer material ou informação para os quais não possua todos os direitos e licenças necessárias;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                (e) não reproduza, copie, modifique, venda, armazene, distribua ou de outro modo explore para quaisquer fins comerciais a QUIVI ou qualquer sua componente (incluindo, mas não limitado a quaisquer materiais ou informação acessível).
                            </p>
                        </li>
                    </ul>
                </li>

                {/* 12º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        12. O CLIENTE ACEITANTE será responsável por qualquer dano, independentemente da sua natureza, que a QUIVI deva suportar em resultado da utilização da QUIVI com desrespeito dos presentes Termos e Condições de Utilização.
                    </p>
                </li>

                {/* 13º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                    13. O CLIENTE ACEITANTE concorda e aceita as seguintes condições:
                    </p>
                    {
                        subMerchantsQuery.data.map(s => (
                            <ul className="ml-4" key={s.id}>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        - {s.name.toUpperCase()} - Custo de {s.setUpFee ?? 0}€, respectivo aos códigos QR a produzir.
                                    </p>
                                </li>
                                <li>
                                    <p className="text-base text-gray-500 dark:text-gray-400">
                                        - {s.name.toUpperCase()} - Desconto da comissão de serviço acordada em {s.transactionFee}% com o prazo de pagamento de 2 dias úteis e incidente sobre o valor de cada transacção de bens ou serviços, cujo pagamento tenha sido efectuado mediante a utilização da plataforma QUIVI.
                                    </p>
                                </li>
                            </ul>
                        ))
                    }
                </li>

                {/* 14º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        14. O arredondamento do cálculo da comissão de serviço será efectuado à centésima, transação a transação, sobre o montante bruto da transação incluindo a gorjeta, com a seguinte regra:
                    </p>
                    <ul className="ml-4">
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                a) Quando a terceira casa decimal for igual ou superior a cinco, o arredondamento é efectuado por excesso;
                            </p>
                        </li>
                        <li>
                            <p className="text-base text-gray-500 dark:text-gray-400">
                                b) Quando a terceira casa decimal for inferior a cinco, o arredondamento é efectuado por defeito.
                            </p>
                        </li>
                    </ul>
                </li>

                {/* 15º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        15. Salvo disposição expressa em contrário entre as Partes, o Contrato entra em vigor na data de subscrição pelas Partes e é celebrado por um período inicial de 1 (um) mês (o "Período Inicial").
                        <br/>
                        Após deste Período Inicial, o Contrato será renovado tacitamente por períodos equivalentes de 1 (um) mês (o "Períodos de Renovação") até ser rescindido por qualquer das Partes. A rescisão será notificada:
                        Pela QUIVI: por carta registada com aviso de receção pelo menos quinze (15) dias antes do fim do Período Inicial ou de qualquer Período de Renovação.
                        Pelo CLIENTE ACEITANTE: por e-mail enviado em qualquer momento à Quivi (sem aviso mínimo necessário) para o endereço: info@quivi.com
                    </p>
                </li>

                {/* 16º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        16. A cada mês, a QUIVI emitirá uma fatura ao CLIENTE ACEITANTE que incluirá os dados das transações realizadas através da Aplicação durante o período decorrido, o valor total das Comissões e Serviços pagos à QUIVI no período mais recente.
                    </p>
                </li>

                {/* 17 Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        17. A QUIVI compromete-se a desenvolver os seus melhores esforços no sentido de manter a QUIVI permanentemente disponível e operacional, não sendo, contudo, responsável por quaisquer interrupções ou impossibilidades de uso que nela venham a ocorrer.  A QUIVI não será responsável por qualquer falha da QUIVI, ou por qualquer dano resultante de circunstâncias alheias ao seu controlo, designadamente, falhas de equipamento, linhas, telefone, vírus, acessos não autorizados, e furto.
                    </p>
                </li>

                {/* 18 Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        18. A QUIVI também não será responsável por qualquer falha da QUIVI, ou por qualquer dano resultante de erros dos operadores, condições climatéricas, tremores de terra ou catástrofes naturais, guerras, greves e outros problemas laborais.
                    </p>
                </li>

                {/* 19º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        19. A QUIVI não é responsável por quaisquer danos decorrentes de vírus que possam alterar o hardware do utilizador devido ao download de quaisquer conteúdos ou programação.
                    </p>
                </li>

                {/* 20º Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        20. A QUIVI reserva-se o direito de obter do CLIENTE ACEITANTE a reparação de todos os prejuízos, designadamente os derivados de responsabilidades exigidas por terceiros, que lhes sejam causados em virtude do uso indevido da QUIVI, designadamente pela violação dos presentes Termos e Condições de Utilização.
                    </p>
                </li>

                {/* 21 Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        21. Acresce IVA à taxa em vigor sobre o valor das comissões ou subscrições.
                    </p>
                </li>

                {/* 22 Term */}
                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        22. Foro aplicável
                    </p>
                </li>

                <li className="mt-4">
                    <p className="text-base text-gray-500 dark:text-gray-400">
                        Qualquer litígio emergente dos presentes Termos e Condições de Utilização será dirimido de acordo com a lei português, pelo Tribunal da Comarca de Lisboa, com expressa renúncia de qualquer outro.
                    </p>
                </li>
            </ul>

            {
                auth.merchantActivated == false &&
                <Button
                    size="md"
                    onClick={save}
                    disabled={merchantsQuery.isFirstLoading || subMerchantsQuery.isFirstLoading}
                    variant="primary"
                >
                    {
                        isSubmitting
                        ?
                        <ClipLoader
                            size={20}
                            cssOverride={{
                                borderColor: "white"
                            }}
                        />
                        :
                        t("common.accept")
                    }
                </Button>
            }
        </ComponentCard>
    </>
}