Problem znalezienia najkrótszej (prostej) ścieżki w grafie.

Musimy znaleźć jak najdłuższą ścieżkę w grafie, przy czym każdy wierzchołek może być odwiedzony maksymalnie jeden raz.

Link: https://en.wikipedia.org/wiki/Longest_path_problem

### Uzycie
```
LongestPathProblem.exe [NAZWA_PLIKU]
```

### Format danych wejściowych
Lista sąsiedztwa, gdzie pierwszą cyfrą jest numer wierzchołka a kolejne cyfry oddzielone spacją określają wierzchołki z którymi jest on połączony. 

Przykład
```
0 1 2 3
1 0 3 4
2 0 3
3 0 1 2 4
4 1 3
```

![przyklad](example-graph.png)

### Format danych wyjściowych
Najdłuższa ścieżka przez graf, podana jako numery wierzchołków oddzielone spacją

Przykład:
```
0 1 4 3 2
```
