import json
import networkx as nx
import matplotlib.pyplot as plt

with open("mem_graph.json") as f:
    data = json.load(f)

G = nx.node_link_graph(data, edges="edges")

plt.figure(figsize=(10,8))
pos = nx.spring_layout(G, k=0.5, iterations=50)
nx.draw_networkx_nodes(G, pos, node_size=50, node_color="skyblue")
nx.draw_networkx_edges(G, pos, alpha=0.3)
nx.draw_networkx_labels(G, pos, font_size=6)

plt.axis("off")
plt.show()
