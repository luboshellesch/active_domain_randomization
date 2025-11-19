# Training Results

This section describes the main parameters used to train the models listed above. The trained models can also be found in
*NICOADR/Assets/Runtime/ML/Models*.

*TODO: Update the parameters described below and explain the reward functions in more detail.*

---

## Models

### Trainer type: PPO

#### 1. `agent_001`

Uses the old reward function; episodes did not terminate correctly, so no meaningful reward curve was recorded in TensorBoard and training stopped early.
Observation vector size: 15.

The model did not train properly.


#### 2. `agent_002`

Uses the old reward function; episodes terminated correctly. Training stopped after 500,000 steps.
Observation vector size: 15.

The model did not train properly because the training agent did not have correct information about the direction to the target.


#### 3. `agent_003`

Reward function 0.1; episodes did not terminate correctly, so no meaningful reward curve was recorded in TensorBoard. Training stopped after 500,000 steps.
Observation vector size: 17.

The model did not train properly because the training agent did not have correct information about the direction to the target.


#### 4. `agent_004`

Reward function 0.1; episodes terminated correctly. Training stopped after 260,000 steps.
Observation vector size: 17.

The model did not train properly because the training agent did not have correct information about the direction to the target.


#### 5. `agent_005`

Reward function 0.3.1 (designed to keep the cumulative reward positive, but it often went negative due to large penalties and a small positive reward term). Episodes terminated correctly. Training stopped after 260,000 steps.
Observation vector size: 16.

`progress_weight = 1f`


#### 6. `agent_006`, `agent_007`, `agent_008` 

Reward function 0.3.2 (designed to keep the cumulative reward positive, with a larger progress weight). Episodes terminated correctly.
Training stopped after 100,000; 500,000; and 6,250,000 steps, respectively.
Observation vector size: 16.

`progress_weight = 5f`

The target can spawn close to NICO.


#### 7. `agent_009`

Reward function 0.3.2 (designed to keep the cumulative reward positive, with a larger progress weight). Episodes terminated correctly. Training stopped after 3,000,000 steps.
Observation vector size: 16.

`progress_weight = 5f`

The target can only spawn at positions beyond NICO’s reachable range. 

Nico can point in generall direction of Target. However, still not precise enough, floating aroumd target not staying on it.

#### 8. `agent_010`
Reward function 0.3.2 (designed to keep the cumulative reward positive, with a larger progress weight). Episodes terminated correctly. Training stopped after 3,000,000 steps.
Observation vector size: 16.

`progress_weight = 2.5f`

The target can only spawn at positions beyond NICO’s reachable range. 

Nico can point in generall direction of Target. However, still not precise enough, floating aroumd target not staying on it.

#### 8. `agent_011`
Reward function 0.3.2 (designed to keep the cumulative reward positive, with a larger progress weight). Episodes terminated correctly. Training stopped after 3,000,000 steps.
Observation vector size: 16.

`progress_weight = 5f`
`learning_rate_schedule = constant`
`learning_rate = 0.0003`

The target can only spawn at positions beyond NICO’s reachable range. 



### Trainer type: SAC