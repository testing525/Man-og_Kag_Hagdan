import json
import os
import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np

# ----------------------------
# Constants
# ----------------------------
ITEMS = ["Pogo Stick", "Bomb", "Stun Gun", "Points Multiplier", "Shield", "Anti Snake Spray", "Points Multiplier"]
ITEM_TO_INDEX = {item: i for i, item in enumerate(ITEMS)}
INDEX_TO_ITEM = {i: item for i, item in enumerate(ITEMS)}

# ----------------------------
# Feature conversion
# ----------------------------
def state_to_features(state):
    return [
        state.get("tiles", 0),
        state.get("round", 1),
        state.get("points", 0),
        int("Shield" in state.get("ownedItems", [])),
        len(state.get("ownedItems", [])) - int("Shield" in state.get("ownedItems", []))
    ]

# ----------------------------
# Load training data
# ----------------------------
def load_training_data():
    path = os.path.join(os.path.dirname(__file__), "training_data.json")
    if not os.path.exists(path):
        print("⚠ No training data found.")
        return None

    with open(path, "r") as f:
        raw = json.load(f)

    training = {}

    # Convert roundItemPurchasesList → dict
    training["roundItemPurchases"] = {}
    for entry in raw.get("roundItemPurchasesList", []):
        round_num = str(entry["round"])
        training["roundItemPurchases"][round_num] = {item["key"]: item["value"] for item in entry["items"]}

    # Convert itemUseFrequencyList → dict
    training["itemUseFrequency"] = {item["key"]: item["value"] for item in raw.get("itemUseFrequencyList", [])}

    # Convert tileItemUsageList → dict
    training["tileItemUsage"] = {}
    for entry in raw.get("tileItemUsageList", []):
        tile = entry["tile"]
        training["tileItemUsage"][tile] = {item["key"]: item["value"] for item in entry["items"]}

    # Copy itemHitEvents
    training["itemHitEvents"] = raw.get("itemHitEvents", [])

    return training

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
    result = {"itemToBuy": item_name}
    with open(path, "w") as f:
        json.dump(result, f, indent=4)

# ----------------------------
# Prepare training samples
# ----------------------------
def generate_training_samples(training):
    X = []
    y = []

    # Use roundItemPurchases
    for round_num, items in training.get("roundItemPurchases", {}).items():
        for item, freq in items.items():
            state = {"tiles": 0, "round": int(round_num), "points": 50, "ownedItems": []}
            X.append(state_to_features(state))
            output = [0] * len(ITEMS)
            if item in ITEM_TO_INDEX:
                output[ITEM_TO_INDEX[item]] = 1
            y.append(output)

    # Use tileItemUsage
    for tile, items in training.get("tileItemUsage", {}).items():
        for item, freq in items.items():
            state = {"tiles": tile, "round": 1, "points": 50, "ownedItems": []}
            X.append(state_to_features(state))
            output = [0] * len(ITEMS)
            if item in ITEM_TO_INDEX:
                output[ITEM_TO_INDEX[item]] = 1
            y.append(output)

    X = torch.tensor(X, dtype=torch.float32)
    y = torch.tensor(y, dtype=torch.float32)
    return X, y

# ----------------------------
# Neural network
# ----------------------------
class BotItemNet(nn.Module):
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
def train_network(X, y, epochs=500, lr=0.01):
    model = BotItemNet(input_size=X.shape[1], output_size=y.shape[1])
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
# Predict item
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
    training = load_training_data()
    if training is None:
        return

    bot_state = load_state()
    if bot_state is None:
        return

    X, y = generate_training_samples(training)
    model = train_network(X, y, epochs=500)

    item = predict_item(model, bot_state)
    write_result(item)

    print("Decision saved:", item)


if __name__ == "__main__":
    run()
