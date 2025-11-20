# Training Results

This section describes the main parameters used to train the models listed above.
The trained models can be found at:
**`NICOADR/Assets/Runtime/ML/Models`**

*TODO: Update the parameters below and provide a detailed explanation of the reward functions.*



## Models

### **Trainer Type: PPO**

---

#### **1. `agent_001`**

Uses the old reward function; episodes did not terminate correctly, so no meaningful reward curve was recorded in TensorBoard, and training stopped early.
**Observation vector size:** 15

**Result:** The model did not train properly.



#### **2. `agent_002`**

Uses the old reward function; episodes terminated correctly.
**Training steps:** 500,000
**Observation vector size:** 15

**Issue:** The agent lacked accurate information about the direction to the target, resulting in poor performance.



#### **3. `agent_003`**

Reward function **0.0.1**; episodes did not terminate correctly, so no meaningful reward curve was recorded in TensorBoard.
**Training steps:** 500,000
**Observation vector size:** 17

**Issue:** The agent did not have correct information about the direction to the target.



#### **4. `agent_004`**

Reward function **0.0.1**; episodes terminated correctly.
**Training steps:** 260,000
**Observation vector size:** 17

**Issue:** The agent still lacked accurate information about the direction to the target.



#### **5. `agent_005`**

Reward function **0.1.0** – designed to keep cumulative rewards positive but often went negative due to strong penalties and a small positive reward.
**Training steps:** 260,000
**Observation vector size:** 16
**`progress_weight = 1f`**



#### **6. `agent_006`, `agent_007`, `agent_008`**

Reward function **0.1.2** – designed to maintain positive cumulative rewards with a larger progress weight.
**Training steps:** 100,000 / 500,000 / 6,250,000
**Observation vector size:** 16
**`progress_weight = 5f`**

The target could spawn close to NICO.



#### **7. `agent_009`**

Reward function **0.1.2**; episodes terminated correctly.
**Training steps:** 3,000,000
**Observation vector size:** 16
**`progress_weight = 5f`**

The target could only spawn beyond NICO’s reachable range.
NICO could orient itself in the general direction of the target but lacked precision — it floated around the target instead of staying on it.



#### **8. `agent_010`**

Reward function **0.1.2**; episodes terminated correctly.
**Training steps:** 3,000,000
**Observation vector size:** 16
**`progress_weight = 2.5f`**

Same setup as `agent_009`. NICO could orient itself toward the target but still lacked precision and failed to stay on it consistently.



#### **9. `agent_011`**

Reward function **0.1.2**
**Training steps:** 3,000,000
**Observation vector size:** 16
**Parameters:**

* `progress_weight = 5f`
* `learning_rate_schedule = constant`
* `learning_rate = 0.0003`

The target could only spawn beyond NICO’s reachable range.



#### **10. `agent_012`, `agent_013`, `agent_014`**

Used new reward function **0.2.1**, **0.2.2**, **0.2.3**, ...
The agent failed to stay on the target and learned to maximize reward in an unintended way.


#### **11. `agent_015`**

Used the new reward function **0.3.1**.
The agent successfully stayed on the target and **did not** exploit the reward function.
**Training duration:** 1,000,000 steps (training to be continued).



### **Trainer Type: SAC**

*(To be filled in)*



