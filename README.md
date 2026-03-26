# Sistema de Automação de Escala de Trabalho

Este projeto consiste em um sistema desktop desenvolvido em **C# utilizando Entity Framework**, com o objetivo de automatizar o gerenciamento, leitura e análise da **escala de trabalho de colaboradores**.

## Funcionalidades principais

O sistema permite:

* Cadastro de colaboradores
* Cadastro e edição de escalas de trabalho
* Alteração de GCM (Grupo/Cargo/Mão de obra)
* Definição de horários e operações
* Identificação automática de códigos de escala através de cores
* Leitura inteligente da escala mensal
* Atualização dinâmica das informações operacionais dos colaboradores

Cada cor presente na escala representa um **código específico**, permitindo que o sistema interprete automaticamente o tipo de jornada do colaborador.

Exemplos:

* TB → Banco de horas
* DN → Day Off de aniversário
* Outros códigos personalizados conforme a operação

## Processamento automático da escala

O sistema realiza automaticamente o cálculo de:

* Horas trabalhadas
* Feriados
* Banco de horas
* Eventos especiais da escala
* Custos operacionais por colaborador

## Análise de custos operacionais

Além do controle da escala, o sistema calcula o **custo individual do colaborador para a empresa**, considerando o **GCM associado ao cargo**, permitindo uma visão estratégica da operação.

## Tecnologias utilizadas

* C#
* .NET Desktop Application



* Entity Framework
* Banco de Dados Relacional
* Processamento lógico baseado em cores para interpretação da escala

## Objetivo do projeto

Automatizar o processo manual de leitura e gerenciamento de escalas operacionais, reduzindo erros humanos, aumentando a produtividade da gestão e fornecendo indicadores precisos sobre jornada de trabalho e custos operacionais.

* TELA DE CADASTRO DO COLABORADOR<img width="1740" height="916" alt="Captura de tela 2026-03-26 204254" src="https://github.com/user-attachments/assets/13e5733c-0808-43c5-b17d-bed8193d9884" />
TELA DO APONTAMENTO

<img width="1702" height="969" alt="Captura de tela 2026-03-26 204439" src="https://github.com/user-attachments/assets/35171a5a-7fb7-4649-b5ac-c31a0c9fbd13" />
