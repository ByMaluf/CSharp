# Instalação e configuração do MySQL Server e MySQL Workbench no Windows

> **Aula 84 — Preparando o servidor de banco de dados para o CashFlow**
>
> Continuação das aulas 82 e 83, que introduziram bancos de dados e a diferença entre modelos relacionais e NoSQL. Nesta aula, o instrutor escolhe o MySQL e demonstra a instalação, a configuração e o primeiro acesso ao servidor em um ambiente Windows.

## Índice

1. Objetivo da aula
2. Por que a API precisa persistir dados?
3. MySQL Server versus MySQL Workbench
4. Download do MySQL Community
5. Instalação personalizada dos dois componentes
6. Configuração para ambiente de desenvolvimento
7. Porta 3306 e conexão local
8. Autenticação e usuário root
9. Configuração do serviço do Windows
10. Finalizando a instalação
11. Primeiro acesso pelo MySQL Workbench
12. Iniciando o servidor manualmente pelo Services
13. Como as peças se conectam
14. Cuidados técnicos e diagnóstico de problemas
15. O que fica para as próximas aulas
16. Resumo e checklist
17. Mapa mental

---

## 1. Objetivo da aula

Nas aulas anteriores, estudamos o conceito de banco de dados e a organização de informações em sistemas relacionais e não relacionais.

Agora, a aplicação precisa de um servidor capaz de **persistir os dados recebidos pela API**.

O instrutor escolhe o **MySQL** e apresenta um procedimento prático no Windows para:

- baixar o instalador da edição Community;
- instalar o MySQL Server e o MySQL Workbench;
- configurar o servidor para desenvolvimento local;
- definir a senha do usuário administrativo;
- configurar o serviço do Windows;
- conectar ao servidor utilizando a interface gráfica;
- aprender a iniciar o serviço manualmente quando necessário.

**Escopo:** a aula prepara e verifica o ambiente de banco de dados. Ainda não é criada uma conexão do CashFlow com o servidor, nem são implementadas operações de persistência em C#.

---

## 2. Por que a API precisa persistir dados?

O professor começa lembrando o fluxo básico de uma API:

```text
Cliente envia requisição HTTP
             |
             v
       API processa
             |
             v
       API devolve resposta
```

Depois que termina o processamento, os dados daquela requisição não ficam automaticamente disponíveis para as próximas operações.

Imagine que uma API recebe o cadastro de uma despesa. Se ela apenas recebe, valida e devolve uma resposta, isso não garante que a despesa possa ser recuperada em outra requisição.

Para consultar, atualizar ou excluir essa despesa posteriormente, é preciso **persistir** os dados em um armazenamento apropriado.

### Persistência de dados

Persistir significa manter informações de forma que possam ser recuperadas depois, além do processamento imediato de uma requisição.

Um fluxo conceitual para o CashFlow seria:

```text
Requisição de cadastro
         |
         v
        API
         |
         v
Validação e processamento
         |
         v
   Banco de dados
         |
         v
Resposta ao cliente

Em uma requisição posterior:
        API -> consulta ao banco -> dados recuperados
```

**Complemento técnico:** uma API também pode manter dados em memória, cache ou outros mecanismos. A afirmação de que a API "esquece" os dados deve ser entendida no contexto de que o processamento de uma requisição, sozinho, não cria persistência durável.

---

## 3. MySQL Server versus MySQL Workbench

O instrutor instala duas ferramentas que possuem responsabilidades diferentes.

| Ferramenta | Função |
|---|---|
| **MySQL Server** | Sistema gerenciador do banco de dados, responsável pelo armazenamento e processamento das operações |
| **MySQL Workbench** | Aplicação gráfica cliente para conectar ao servidor, consultar informações e administrar objetos do banco |

### MySQL Server

É o componente que efetivamente mantém e gerencia os bancos de dados.

É a ele que a API precisará se conectar futuramente para armazenar e recuperar informações.

### MySQL Workbench

É uma interface visual para trabalhar com um servidor MySQL.

O instrutor explica que o Workbench permite:

- conectar ao servidor;
- visualizar bancos, tabelas e registros;
- inserir, atualizar e excluir registros;
- criar e excluir tabelas;
- executar operações sobre os dados.

### Relação entre servidor e cliente

```text
MySQL Workbench ─── conexão ───> MySQL Server
                                      |
                                      v
                                Bancos e dados
```

**Ponto-chave:** o Workbench não substitui o Server. A interface gráfica envia operações ao servidor, que as processa.

O servidor pode funcionar sem que o Workbench esteja aberto. Por outro lado, o Workbench precisa de um servidor acessível para consultar e modificar dados.

---

## 4. Download do MySQL Community

A aula começa no site oficial do MySQL.

O procedimento apresentado é:

1. Localizar a seção **MySQL Community**.
2. Selecionar **MySQL Installer for Windows**.
3. Escolher a última opção de download disponível na tela da gravação.
4. Clicar em **Download**.
5. Na página que oferece login, selecionar **No thanks, just start my download**.
6. Após o download, abrir o instalador.

A escolha do MySQL Installer for Windows permite ao instrutor selecionar tanto o servidor quanto o Workbench no mesmo procedimento de instalação.

**Contexto da gravação:** as páginas de download, os nomes dos instaladores e as versões disponíveis podem mudar. Este resumo documenta o fluxo apresentado no vídeo, não uma afirmação de que o mesmo instalador permanece atualmente disponível para todas as versões.

---

## 5. Instalação personalizada dos dois componentes

Depois de abrir o instalador, o instrutor seleciona a opção **Custom**.

O instalador apresenta alternativas para instalar somente servidor, somente cliente ou outros conjuntos de produtos. A opção personalizada permite selecionar os componentes desejados.

### Produtos selecionados

| Produto | Versão utilizada na gravação |
|---|---|
| MySQL Server | **8.0.35** |
| MySQL Workbench | **8.0.34** |

### Passo a passo

1. Selecionar **Custom**.
2. Clicar em **Next**.
3. Expandir a categoria **MySQL Servers**.
4. Localizar e selecionar **MySQL Server 8.0.35**.
5. Usar a seta verde para transferir o produto à lista da direita.
6. Expandir a categoria **Applications**.
7. Selecionar **MySQL Workbench 8.0.34**.
8. Transferi-lo também à lista de produtos que serão instalados.
9. Clicar em **Next**.
10. Confirmar os produtos e clicar em **Execute**.

### O que representam as duas listas?

```text
PRODUTOS DISPONÍVEIS           PRODUTOS SELECIONADOS
MySQL Server 8.0.35   ----->    MySQL Server 8.0.35
MySQL Workbench 8.0.34 ---->    MySQL Workbench 8.0.34
```

O instrutor aguarda a conclusão e observa que ambos os produtos foram instalados com sucesso.

**Importante:** as versões 8.0.35 e 8.0.34 são as escolhidas na gravação. O vídeo não estabelece essas versões como requisito absoluto do projeto nem as comprova como versões atuais.

---

## 6. Configuração para ambiente de desenvolvimento

Após instalar os componentes, o instalador solicita a configuração do MySQL Server.

O professor avança até a tela que permite selecionar o perfil de utilização do servidor.

A escolha é o perfil destinado a um **computador de desenvolvimento** (*Development Computer*).

### Por que essa escolha?

O MySQL será executado na própria máquina utilizada para programar e testar a API, e não em um computador dedicado exclusivamente ao banco de dados.

A aula está preparando um ambiente local:

```text
Computador de desenvolvimento
|
+-- Visual Studio
+-- API CashFlow
+-- MySQL Server
+-- MySQL Workbench
```

**Complemento técnico:** perfis de configuração podem influenciar o uso de recursos do servidor, como memória. O instrutor não altera parâmetros detalhados de desempenho nesta aula.

---

## 7. Porta 3306 e conexão local

O instrutor mantém a porta padrão do servidor:

```text
Porta: 3306
```

A porta é utilizada para identificar o endpoint de rede pelo qual um cliente se comunica com o serviço MySQL.

Como o servidor será executado na máquina de desenvolvimento, a referência de conexão local é:

```text
Host: localhost
Porta: 3306
```

### O que é localhost?

É a referência à própria máquina na qual o programa está sendo executado.

### O que é uma porta?

É um identificador utilizado pela comunicação de rede para direcionar uma conexão a um serviço.

```text
MySQL Workbench
        |
        v
localhost:3306
        |
        v
MySQL Server
```

O vídeo mantém a configuração padrão de porta e segue para as opções de autenticação.

**Observação técnica:** 3306 é a porta padrão usual do protocolo MySQL clássico, mas pode ser alterada. Se o servidor utilizar outra porta, o cliente precisa utilizar a configuração correspondente.

---

## 8. Autenticação e usuário root

O professor seleciona a primeira opção de autenticação apresentada pelo instalador e continua para a definição das credenciais.

O MySQL disponibiliza um usuário administrativo chamado **root**.

### Qual é a função do root?

O usuário root possui amplos privilégios administrativos no servidor. Na demonstração, é utilizado para realizar a configuração e o primeiro acesso ao MySQL.

O instalador solicita a criação e confirmação da senha desse usuário.

### Dados configurados na aula

| Informação | Configuração |
|---|---|
| Usuário | root |
| Senha | Definida durante a instalação |
| Ambiente | Desenvolvimento local |

O instrutor usa uma senha demonstrativa contendo letras maiúsculas e minúsculas, números e um caractere especial. O instalador indica que ela atende aos critérios apresentados na tela.

**Cuidado:** a senha da gravação é pública e previsível. Não a reutilize como credencial real. Para o seu ambiente, escolha uma senha própria e mantenha-a fora do código-fonte e do Git.

### Complemento técnico: autenticação versus autorização

- **Autenticação:** comprovar a identidade, por exemplo, fornecendo usuário e senha.
- **Autorização:** determinar quais operações esse usuário pode realizar.

O root é adequado para a configuração administrativa demonstrada. Em uma aplicação real, é recomendável utilizar uma conta própria com os privilégios necessários, em vez de conceder acesso administrativo irrestrito à API.

A aula ainda não cria essa conta de aplicação.

---

## 9. Configuração do serviço do Windows

O instalador oferece a possibilidade de configurar o MySQL Server como um **serviço do Windows**.

### O que significa isso?

Um serviço é um programa gerenciado pelo sistema operacional, que pode ser iniciado e interrompido sem depender de uma janela gráfica aberta.

No vídeo, o MySQL é registrado como um serviço.

O instrutor, entretanto, **desmarca a opção de iniciar o servidor automaticamente com o Windows**.

### Motivo apresentado

Economizar recursos quando o banco não estiver sendo utilizado.

O professor prefere iniciar o MySQL somente quando precisar trabalhar com a aplicação ou acessar os dados.

### Comparação

| Inicialização automática | Inicialização manual |
|---|---|
| O serviço é iniciado junto com o Windows | O serviço é iniciado quando solicitado |
| O servidor fica disponível sem ação adicional | É necessário iniciar o serviço antes de usar o banco |
| Pode consumir recursos mesmo quando não está em uso | Permite evitar esse consumo quando não é necessário |

### Atenção ao efeito da configuração

Desmarcar a inicialização automática **não desinstala o servidor nem impede que ele seja executado**.

Significa apenas que, após uma reinicialização, será necessário iniciá-lo manualmente conforme a configuração escolhida.

---

## 10. Finalizando a instalação

Após definir o perfil, a porta, a autenticação e o serviço, o instrutor avança pelas telas restantes.

O procedimento mostrado é:

1. Clicar em **Next** até a etapa de aplicação das configurações.
2. Clicar em **Execute**.
3. Aguardar a conclusão da configuração.
4. Clicar em **Finish**.
5. Avançar até a conclusão do instalador.
6. Manter selecionada a opção de abrir o MySQL Workbench.
7. Finalizar.

O Workbench é aberto automaticamente e apresenta uma conexão local com o servidor recém-configurado.

**Resultado observado:** instalação e configuração concluídas com sucesso no ambiente do professor.

---

## 11. Primeiro acesso pelo MySQL Workbench

Ao abrir o Workbench, o instrutor encontra uma conexão já disponível para o servidor local.

Ele seleciona a conexão, e o programa solicita a senha do usuário **root**.

### Dados utilizados

```text
Host: servidor local
Porta: 3306
User: root
Password: senha definida na instalação
```

Após informar a senha, o Workbench consegue estabelecer a conexão.

### O que esse primeiro acesso comprova?

Que o servidor está disponível e aceita a autenticação com as credenciais configuradas.

```text
Workbench seleciona conexão
             |
             v
Solicita a senha de root
             |
             v
Autentica no MySQL Server
             |
             v
Acesso ao servidor concedido
```

A aula **não cria as tabelas do CashFlow** nem executa um cadastro de despesa nessa etapa.

O primeiro acesso serve para confirmar que o ambiente está pronto para as próximas implementações.

---

## 12. Iniciando o servidor manualmente pelo Services

Como o instrutor optou por não iniciar automaticamente o MySQL com o Windows, ele demonstra como encontrar e iniciar o serviço.

### Procedimento no Windows

1. Abrir a pesquisa do sistema operacional.
2. Pesquisar **Services** ou **Serviços**.
3. Abrir o aplicativo de gerenciamento de serviços.
4. Localizar o serviço **MySQL80**.
5. Verificar seu status.
6. Quando estiver parado, clicar com o botão direito e selecionar **Start**.
7. Confirmar que o status passou a **Running**.

O nome MySQL80 é o exibido na máquina do instrutor. O nome do serviço pode variar se outra versão ou configuração for utilizada.

### Estados apresentados

| Estado | Significado |
|---|---|
| Running | O serviço está em execução |
| Parado | O servidor não está disponível por meio daquele serviço |
| Start | Comando para iniciar o serviço |
| Stop | Comando para interromper o serviço |

### Fluxo após reiniciar o computador

```text
Ligar o computador
        |
        v
Precisa usar o MySQL?
        |
       Sim
        |
        v
Abrir Services / Serviços
        |
        v
Selecionar MySQL80
        |
        v
Start
        |
        v
Status Running
        |
        v
Servidor disponível
```

**Ponto-chave:** abrir apenas o Workbench não garante que o MySQL Server esteja em execução. Se o serviço estiver parado, primeiro é necessário iniciar o servidor.

---

## 13. Como as peças se conectam

O objetivo final da instalação é permitir que a API e ferramentas administrativas usem o mesmo servidor.

```text
                  COMPUTADOR LOCAL
                         |
       +-----------------+-----------------+
       |                                   |
       v                                   v
 MySQL Workbench                      API CashFlow
 Cliente gráfico                     Cliente futuro
       |                                   |
       +-----------------+-----------------+
                         |
                         v
                MySQL Server :3306
                         |
                         v
                   Banco de dados
                         |
                         v
                Registros persistidos
```

O Workbench permite administrar visualmente o banco. A API, quando integrada posteriormente, poderá realizar operações programaticamente.

**Importante:** a comunicação da API com o MySQL é uma etapa futura. O vídeo verifica somente o acesso pelo Workbench.

---

## 14. Cuidados técnicos e diagnóstico de problemas

Esta seção complementa a gravação para ajudar a aplicar o procedimento sem confundir os componentes.

| Sintoma | Verificação inicial |
|---|---|
| Workbench abre, mas não conecta | Conferir se o serviço MySQL está em execução |
| Erro de autenticação | Conferir usuário e senha definidos na instalação |
| Conexão local não encontrada | Conferir host e porta configurados |
| Servidor deixa de estar disponível após reiniciar o computador | Verificar a inicialização manual escolhida |
| Serviço MySQL80 não encontrado | Conferir o nome do serviço e a instalação realizada |

Essas são orientações gerais de diagnóstico, não erros que o professor tenha enfrentado na gravação.

### Cuidados adicionais

- As versões utilizadas no vídeo são referências históricas, não uma recomendação de instalar versões antigas sem avaliar suporte e atualizações.
- Não utilizar a senha demonstrativa do curso como senha real.
- Não armazenar credenciais no repositório Git.
- Evitar expor o servidor local desnecessariamente à rede.
- Para a futura conexão da API, distinguir as credenciais administrativas das credenciais destinadas à aplicação.

---

## 15. O que fica para as próximas aulas?

Ao final, o professor afirma que o MySQL está instalado e funcionando.

A estrutura disponível permite que, em aulas posteriores, a API seja conectada ao servidor e comece a realizar operações como:

- inserir dados;
- consultar registros;
- atualizar informações;
- excluir registros.

Ainda não foram apresentados a configuração da conexão em C#, os pacotes de acesso a dados ou as implementações de persistência.

---

## 16. Resumo e checklist

### Principais aprendizados

- O processamento de uma requisição HTTP não garante persistência de dados.
- O **MySQL Server** é responsável por gerenciar os dados.
- O **MySQL Workbench** é a interface gráfica cliente.
- O instrutor utiliza o **MySQL Community Installer for Windows**, com instalação personalizada.
- As versões da demonstração são **Server 8.0.35** e **Workbench 8.0.34**.
- A configuração escolhida é destinada ao desenvolvimento local.
- A porta mantida é **3306**.
- O usuário administrativo utilizado é **root**.
- A senha é criada durante a configuração.
- O servidor é instalado como serviço do Windows, mas a inicialização automática é desmarcada.
- O primeiro acesso pelo Workbench confirma que o servidor está acessível.
- O serviço **MySQL80** pode ser iniciado manualmente pelo aplicativo Serviços do Windows.

### Checklist de revisão prática

- [ ] MySQL Server instalado.
- [ ] MySQL Workbench instalado.
- [ ] Perfil de desenvolvimento selecionado.
- [ ] Host e porta conhecidos.
- [ ] Senha própria configurada.
- [ ] Serviço do Windows configurado.
- [ ] Serviço em execução quando necessário.
- [ ] Conexão local pelo Workbench funcionando.

---

## 17. Mapa mental

```text
                    AULA 84
                       |
              PERSISTIR DADOS DA API
                       |
                       v
                 MySQL Community
                       |
                  Instalação Custom
                       |
             +---------+---------+
             |                   |
         MySQL Server      MySQL Workbench
           8.0.35              8.0.34
             |                   |
             v                   |
        Configuração             |
             |                   |
        Development              |
        localhost:3306           |
        root + senha             |
             |                   |
             v                   |
      Serviço MySQL80            |
      Inicialização manual       |
             |                   |
             +---------+---------+
                       |
                       v
          Primeiro acesso bem-sucedido
                       |
                       v
               Ambiente preparado
                       |
                       v
             Integração futura da API
```

**Conclusão:** esta aula prepara o ambiente necessário para a persistência de dados no CashFlow. O servidor MySQL é instalado e configurado, o Workbench permite validar o acesso e o gerenciamento manual do serviço deixa claro como disponibilizar o banco sempre que for necessário.
