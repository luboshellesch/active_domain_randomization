# Training Results
Here we describe some of the parameters used to train the models above. Trained models can be also found in *NICOADR/Assets/Runtime/ML/Models*

*TODO Update described parameters, explain rewards*  

## Models ##
**1. nico_new_agent_1**

Old reward, episodes not terminating correctly - no TensorBoard reward, early stop. Observation vector space 15.

Model not trained properly. 

**2. nico_new_agent_2**

Old reward, episodes correctly terminating. Stop after 500000 steps. Observation vector space 15.

Model not trained properly - training agent did not have correct information about the direction to the Target.

**3. nico_new_agent_3**

Reward 0.1, episodes not terminating correctly - no TensorBoard reward. Stop after 500000 steps. Observation vector space 17.

Model not trained properly - training agent did not have correct information about the direction to the Target.