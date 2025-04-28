import os
import matplotlib.pyplot as plt
from matplotlib import rcParams
from typing import List, Tuple

class FrequencyVisualizer:
    def __init__(self):
        rcParams['font.family'] = 'Arial'
        self.max_items = 100 
        self.figsize = (15, 8)  
        self.bar_width = 0.45 
        self.dpi = 300  

    def read_frequency_data(self, filepath: str) -> List[Tuple[str, float, float]]:
        data = []
        with open(filepath, 'r', encoding='utf-8') as f:
            for line in f:
                if len(data) >= self.max_items:
                    break
                if line.strip():
                    parts = line.strip().split()
                    if len(parts) >= 3:
                        try:
                            label = parts[0]
                            actual = float(parts[1].replace(',', '.'))
                            expected = float(parts[2].replace(',', '.'))
                            data.append((label, expected, actual))
                        except ValueError:
                            continue
        return data

    def create_frequency_plot(self, data: List[Tuple[str, float, float]], 
                            title: str, save_path: str) -> None:
        if not data:
            print(f"No data to plot for {title}")
            return

        labels, expected, actual = zip(*data)
        x_pos = range(len(labels))

        plt.figure(figsize=self.figsize)
        
        plt.bar(
            [x - self.bar_width/2 for x in x_pos], 
            expected, 
            width=self.bar_width, 
            label='Ожидаемая частота', 
            color='grey'
        )
        plt.bar(
            [x + self.bar_width/2 for x in x_pos], 
            actual, 
            width=self.bar_width, 
            label='Текущая частота', 
            color='red'
        )

        plt.title(title, fontsize=14)
        plt.xlabel('Параметры', fontsize=12)
        plt.ylabel('Частота', fontsize=12)
        plt.xticks(x_pos, labels, rotation=90, fontsize=8)
        plt.legend()
        plt.tight_layout()
        plt.grid(True, axis='y', linestyle='--', alpha=0.7)
        
        plt.savefig(save_path, dpi=self.dpi, bbox_inches='tight')
        plt.close()
        print(f"График сохранен: {save_path}")

    def analyze_all(self) -> None:
        results_dir = os.path.join(os.getcwd(), "../Results")
        # os.makedirs(results_dir, exist_ok=True)

        bigrams_data = os.path.join(results_dir, "gen-1-analysis.txt")
        bigrams_plot = os.path.join(results_dir, "gen-1.png")
        if os.path.exists(bigrams_data):
            data = self.read_frequency_data(bigrams_data)
            self.create_frequency_plot(data, 'Распределение частот для биграмм', bigrams_plot)

        words_data = os.path.join(results_dir, "gen-2-analysis.txt")
        words_plot = os.path.join(results_dir, "gen-2.png")
        if os.path.exists(words_data):
            data = self.read_frequency_data(words_data)
            self.create_frequency_plot(data, 'Распределение частот для слов', words_plot)


if __name__ == "__main__":
    visualizer = FrequencyVisualizer()
    visualizer.analyze_all()