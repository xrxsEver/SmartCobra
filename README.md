# Old snake game ... 
The primary research focus of this project is to explore different algorithms and strategies to solve the Snake game with a high success rate.
The classic Hamiltonian cycle algorithm(The Longest Path solution), which guarantees a win by following a predetermined path, was considered.

### Hamiltonian cycle

![unicursalmazeloop](https://github.com/user-attachments/assets/86d22742-9ccb-4a88-b485-12f814a698b7)

However, due to its predictability and lack of challenge, a custom algorithm was developed that aims to balance between exploration and optimization. Although the algorithm does not guarantee a complete solution, it has successfully covered up to **75%** of the game map in various runs, making it a compelling alternative.

![sn2](https://github.com/user-attachments/assets/8d6aff12-91e5-47db-9031-526a0443f3fd)

the lag is for 8fps gif ☕

## Scope
This project seeks to develop an AI-driven snake that intelligently navigates the grid to consume as much food as possible without colliding with the walls or its own body. The algorithm focuses on finding an optimal path towards the food while dynamically adjusting its strategy when no clear path exists. This is achieved through a combination of **pathfinding, patrolling, and collision avoidance techniques**.

## Difficulty
The difficulty of this project lies in creating an algorithm that can adapt to the dynamic environment of the Snake game. Unlike the Hamiltonian cycle, which is deterministic, the custom algorithm must balance between aggressive food-seeking and defensive patrolling to avoid early termination. The challenge is further compounded by the need to handle edge cases where food is in unreachable locations, requiring the snake to adopt a patrolling behavior to maximize the covered area.

![sn](https://github.com/user-attachments/assets/4942698c-ef48-411e-8907-b4e562d1e199)

## Implementation
The algorithm was implemented in Unity using C#. Key features include:

- **Pathfinding with A\***: The snake uses an A* inspired algorithm to find the shortest path to the food. If a path is not found, it switches to a patrol mode, covering the grid methodically to maximize area coverage.
- **Dynamic Patrol Strategy**: When a direct path is unavailable, the snake enters a patrol mode that evaluates potential directions based on open space and safety.
- **Data Logging and Analysis**: The game logs essential metrics like time elapsed, food eaten, and grid coverage percentage, which are saved in a CSV file. 

## Results and Observations
The attached CSV file, `Food_Eaten_Ratio_by_Total_Food_Spot.csv`, records the performance of the algorithm across different grid sizes and configurations. The data shows that while the algorithm does not complete the map 100% of the time, it achieves respectable coverage with an average completion rate of around **47-61%**, peaking at **75%** in the best run.

![runs](https://github.com/user-attachments/assets/6e98a226-8965-4935-b31b-a8cf9b83ede7)


## References


https://johnflux.com/2015/05/02/nokia-6110-part-3-algorithms/
