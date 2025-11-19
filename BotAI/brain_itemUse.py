import json
import os
import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np

# ----------------------------
# Constants
# ----------------------------
USE_ITEMS = ["Bomb", "Pogo Stick", "Points Multiplier", "Stun Gun"]  # Items the bot can use
ITEM_TO_INDEX = {item: i for i, item in enumerate(USE_ITEMS)}
INDEX_TO_ITEM = {i: item for i, item in enumerate(USE_ITEMS)}

# ----------------------------
# Feature conversion
# ----------------------------
def state_to_features(state):
    """
    Convert bot state to feature vector for ML.
    """
    return [
        state.get("tiles", 0),                   # Current tile
        state.get("round", 1),                   # Current round
        state.get("points", 0),                  # Points
        int("Shield" in state.get("ownedItems", [])),  # Binary flag if shield owned
        int("Anti Snake Spray" in state.get("ownedItems", [])),  # Binary flag if Anti Snake Spray owned
        int("Bomb" in state.get("ownedItems", [])),  # Binary flag if Bomb owned
        int("Pogo Stick" in state.get("ownedItems", [])),  # Binary flag if Pogo Stick owned
        state.get("tilesToFirstPlayer", 0),      # Optional: distance to player ahead
        state.get("tilesToSnake", 0)             # Optional: distance to nearest snake
    ]

# ----------------------------
# Load bot state
# ----------------------------
def load_state():
    path = os.path.join(os.path.dirname(__file__), "state.json")
    if not os.path.exists(path):
        print("⚠ No state.json found.")
        return None

    with open(path, "r") as f:
        return json.load(f)

# ----------------------------
# Save AI decision
# ----------------------------
def write_result(item_name):
    path = os.path.join(os.path.dirname(__file__), "result.json")
    result = {"itemToUse": item_name}
    with open(path, "w") as f:
        json.dump(result, f, indent=4)

# ----------------------------
# Generate training samples
# ----------------------------
def generate_training_samples(bot_state):
    """
    For now, use simple one-hot from ownedItems.
    Can be replaced later with actual ML training data.
    """
    X = []
    y = []

    owned = bot_state.get("ownedItems", [])
    for item in USE_ITEMS:
        state_features = state_to_features(bot_state)
        X.append(state_features)

        output = [0] * len(USE_ITEMS)
        if item in owned:
            output[ITEM_TO_INDEX[item]] = 1
        y.append(output)

    X = torch.tensor(X, dtype=torch.float32)
    y = torch.tensor(y, dtype=torch.float32)
    return X, y

# ----------------------------
# Neural network
# ----------------------------
class BotUseItemNet(nn.Module):
    def __init__(self, input_size, output_size):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_size, 16),
            nn.ReLU(),
            nn.Linear(16, output_size),
            nn.Softmax(dim=1)
        )

    def forward(self, x):
        return self.net(x)

# ----------------------------
# Train network
# ----------------------------
def train_network(X, y, epochs=300, lr=0.01):
    model = BotUseItemNet(input_size=X.shape[1], output_size=y.shape[1])
    criterion = nn.MSELoss()
    optimizer = optim.Adam(model.parameters(), lr=lr)

    for epoch in range(epochs):
        optimizer.zero_grad()
        output = model(X)
        loss = criterion(output, y)
        loss.backward()
        optimizer.step()

    return model

# ----------------------------
# Predict item to use
# ----------------------------
def predict_item(model, bot_state):
    features = torch.tensor([state_to_features(bot_state)], dtype=torch.float32)
    probs = model(features).detach().numpy()[0]
    best_index = np.argmax(probs)
    return INDEX_TO_ITEM[best_index]

# ----------------------------
# MAIN
# ----------------------------
def run():
    bot_state = load_state()
    if bot_state is None:
        return

    # Generate training samples from current bot state (placeholder)
    X, y = generate_training_samples(bot_state)

    # Train simple ML model
    model = train_network(X, y, epochs=300)

    # Predict best item to use
    item = predict_item(model, bot_state)
    write_result(item)

    print("Item to use saved:", item)

if __name__ == "__main__":
    run()
