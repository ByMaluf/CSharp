# Roadmap de C (30–40 horas)

Este roadmap foi criado para fortalecer os **fundamentos de programação**, especialmente para desenvolvedores que utilizam linguagens de alto nível como C#, Java, Python ou JavaScript.

Tempo total estimado: **30–40 horas**

---

# Fase 1 — Fundamentos da linguagem (6h)

## Conceitos

- Como funciona um programa em C
- Estrutura de um programa
- Compilação (`gcc` / `clang`)
- Função `main()`

## Sintaxe básica

- variáveis
- tipos primitivos
- operadores

### Tipos principais

```c
int
float
double
char
```

### Exercícios

- calculadora simples
- conversão de temperatura
- média de números

---

# Fase 2 — Controle de fluxo (4h)

## Condicionais

```c
if
else
else if
switch
```

## Loops

```c
for
while
do while
```

### Exercícios

- verificar se número é par ou ímpar
- tabuada
- cálculo de fatorial
- contador regressivo

---

# Fase 3 — Funções (3h)

## Conceitos

- declaração de função
- parâmetros
- retorno

### Exemplo

```c
int soma(int a, int b){
    return a + b;
}
```

### Exercícios

- função de soma
- função de média
- função para verificar número primo

---

# Fase 4 — Arrays e Strings (4h)

## Arrays

```c
int numeros[10];
```

## Strings

Em C, string é representada como um array de caracteres:

```c
char nome[50];
```

## Funções comuns

```c
strlen()
strcpy()
strcmp()
```

### Exercícios

- inverter string
- contar letras de uma frase
- encontrar maior número de um array

---

# Fase 5 — Ponteiros (8h)

Esta é uma das partes **mais importantes da linguagem C**.

## Conceitos

- endereço de memória
- operador `&`
- operador `*`

### Exemplo

```c
int x = 10;
int *p = &x;
```

## O que estudar

- ponteiros básicos
- ponteiro para ponteiro
- ponteiros e arrays
- passagem por referência

### Exercícios

- trocar valores de duas variáveis
- percorrer array usando ponteiros
- implementar função `swap`

---

# Fase 6 — Structs (3h)

Structs permitem criar **tipos personalizados**.

### Exemplo

```c
struct Pessoa {
    char nome[50];
    int idade;
};
```

### Exercícios

- cadastro de pessoas
- lista de alunos
- estrutura de produtos

---

# Fase 7 — Alocação de memória (5h)

Aqui você aprende **como a memória funciona na prática**.

## Funções importantes

```c
malloc()
calloc()
realloc()
free()
```

## Conceitos

- stack vs heap
- vazamento de memória
- alocação dinâmica

### Exercícios

- vetor dinâmico
- lista dinâmica de números
- redimensionar array com `realloc`

---

# Fase 8 — Estruturas de dados básicas (5h)

Aplicação prática dos conceitos aprendidos.

## Implementar

- lista encadeada
- pilha
- fila

### Exemplo conceitual

```c
struct Node {
    int valor;
    struct Node *next;
};
```

---

# Distribuição de tempo

| Fase | Tempo |
|-----|------|
| Fundamentos | 6h |
| Controle de fluxo | 4h |
| Funções | 3h |
| Arrays e strings | 4h |
| Ponteiros | 8h |
| Structs | 3h |
| Memória dinâmica | 5h |
| Estruturas de dados | 5h |

**Tempo total aproximado: 38 horas**

---

# Ferramentas recomendadas

## Compilador

```
gcc
```

## Editor

```
VS Code
```

## Compilar

```bash
gcc programa.c -o programa
```

## Executar

```bash
./programa
```

---

# O que você terá aprendido após este roadmap

- funcionamento da memória
- ponteiros
- stack vs heap
- estruturas de dados
- como linguagens modernas funcionam internamente

Esse conhecimento melhora significativamente a capacidade de programar em linguagens como:

- C#
- Java
- Python
- JavaScript