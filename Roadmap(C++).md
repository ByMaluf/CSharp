# Roadmap de C++ (30–40 horas)

Este roadmap foca em **C++ moderno (C++17/C++20)** e é voltado para desenvolvedores que já têm alguma base de programação e querem aprender **C++ de forma estruturada**.

Tempo total estimado: **30–40 horas**

---

# Fase 1 — Fundamentos da linguagem (5h)

## Conceitos

- estrutura de um programa em C++
- função `main`
- compilação com `g++` ou `clang++`
- namespaces
- entrada e saída

### Exemplo

```cpp
#include <iostream>

int main() {
    std::cout << "Hello, world!" << std::endl;
    return 0;
}
```

## Tipos básicos

```cpp
int
double
char
bool
```

## Exercícios

- imprimir mensagens
- somar dois números
- converter temperatura
- calcular média

---

# Fase 2 — Controle de fluxo (4h)

## Condicionais

```cpp
if
else
else if
switch
```

## Loops

```cpp
for
while
do while
```

### Exercícios

- verificar número par ou ímpar
- tabuada
- cálculo de fatorial
- contador regressivo

---

# Fase 3 — Funções (3h)

## Conceitos

- declaração de funções
- parâmetros
- retorno
- sobrecarga de funções

### Exemplo

```cpp
int soma(int a, int b){
    return a + b;
}
```

### Exercícios

- função de média
- função de número primo
- função de potência

---

# Fase 4 — Arrays, Strings e Vetores (5h)

## Arrays

```cpp
int numeros[10];
```

## Strings modernas

```cpp
#include <string>

std::string nome = "Ana";
```

## Vetores (STL)

```cpp
#include <vector>

std::vector<int> numeros;
```

### Exercícios

- inverter string
- encontrar maior valor em vetor
- calcular média de vetor

---

# Fase 5 — Ponteiros e Referências (5h)

## Ponteiros

```cpp
int x = 10;
int* p = &x;
```

## Referências

```cpp
int x = 10;
int& ref = x;
```

## Passagem por referência

```cpp
void trocar(int &a, int &b) {
    int temp = a;
    a = b;
    b = temp;
}
```

### Exercícios

- implementar função swap
- percorrer vetor com ponteiro
- modificar variável por referência

---

# Fase 6 — Programação Orientada a Objetos (8h)

## Conceitos

- classes
- atributos
- métodos
- encapsulamento

### Exemplo

```cpp
class Pessoa {
public:
    std::string nome;
    int idade;

    void apresentar() {
        std::cout << "Olá, meu nome é " << nome << std::endl;
    }
};
```

## Tópicos

- construtores
- destrutores
- herança
- polimorfismo

### Exercícios

- classe Pessoa
- classe Produto
- classe ContaBancaria

---

# Fase 7 — STL (Standard Template Library) (5h)

## Containers

```cpp
std::vector
std::list
std::map
std::set
```

## Exemplo

```cpp
#include <vector>

std::vector<int> numeros = {1,2,3,4};
```

## Algoritmos

```cpp
#include <algorithm>

std::sort(v.begin(), v.end());
```

### Exercícios

- ordenar vetor
- buscar elemento
- contar elementos

---

# Fase 8 — Gerenciamento de memória moderno (4h)

## Smart pointers

```cpp
std::unique_ptr
std::shared_ptr
```

### Exemplo

```cpp
#include <memory>

std::unique_ptr<int> p = std::make_unique<int>(10);
```

## Conceitos

- RAII
- gerenciamento automático de recursos

---

# Distribuição de tempo

| Fase | Tempo |
|-----|------|
| Fundamentos | 5h |
| Controle de fluxo | 4h |
| Funções | 3h |
| Arrays / Strings / Vetores | 5h |
| Ponteiros e referências | 5h |
| Orientação a objetos | 8h |
| STL | 5h |
| Memória moderna | 4h |

**Tempo total aproximado: 39 horas**

---

# Ferramentas recomendadas

## Compilador

```
g++
```

ou

```
clang++
```

## Editor

```
VS Code
```

---

# Compilar programa

```bash
g++ programa.cpp -o programa
```

# Executar

```bash
./programa
```

---

# O que você terá aprendido após este roadmap

- fundamentos da linguagem C++
- programação orientada a objetos
- uso da STL
- gerenciamento moderno de memória
- containers e algoritmos
- boas práticas de C++ moderno