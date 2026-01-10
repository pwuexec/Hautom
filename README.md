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

## Limitações

* Compatível apenas com faturas da **Endesa**
* Suporta apenas tarifários simples (sem bi-horário ou tri-horário)
* Todo o processamento é feito localmente, garantindo privacidade dos dados

## Como usar

1. Coloque as faturas em PDF na pasta configurada
2. Execute a aplicação
3. O consumo e os custos são analisados e guardados automaticamente
