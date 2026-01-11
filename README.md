# Hautom – Análise de Consumo Endesa

O **Hautom** é uma ferramenta open-source para **analisar o consumo de eletricidade a partir de faturas da Endesa**.
O objetivo é ajudar a compreender custos reais, consumos e descontos aplicados, de forma clara e transparente, sem ruído comercial.

![mockup.png](media/mockup.png)

## O que faz

* Analisa consumos (kWh) e períodos faturados
* Calcula valores totais e identifica descontos aplicados
  *(Tarifa Social, meses de oferta, entre outros)*
* Mantém um histórico de consumo e custos ao longo do tempo
* Evita faturas duplicadas através de verificação automática
* **Simulação de tarifas** – permite comparar quanto pagaria com um preço de kWh diferente

## Limitações

* Compatível apenas com faturas da **Endesa**
* Suporta apenas tarifários simples (sem bi-horário ou tri-horário)
* Todo o processamento é feito localmente, garantindo privacidade dos dados

## Como usar

1. Coloque as faturas em PDF na pasta configurada
2. Execute a aplicação
3. O consumo e os custos são analisados e guardados automaticamente

## Simulação de Tarifas

O painel de simulação permite testar diferentes preços de kWh e ver o impacto nos custos:

1. Introduza o preço por kWh no campo de simulação (ex: `1500` = 0.1500 €/kWh)
2. Os valores são recalculados automaticamente em toda a interface
3. Os cards mostram o badge **SIMULAÇÃO** quando ativo
4. A tabela destaca as linhas simuladas com fundo azul e badge **SIM**
5. Veja a poupança ou custo adicional em tempo real
6. Use o botão **Limpar** para voltar aos valores reais

Ideal para comparar tarifas de diferentes fornecedores ou avaliar propostas comerciais.
