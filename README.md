# Active Domain Randomization for Training the Virtual Twin of NICO in Unity

This repository contains the source code, Unity project, and experimental setup for my Master's thesis:  
**"Active Domain Randomization for Training the Virtual Twin of NICO in Unity"**.

The project was developed using Unity version 2022.3.4f1, and the robot was trained with ML-Agents version 2.0.2.

## Objectives

The aim of this thesis is to implement the ADR (Active Domain Randomization) into robotic infrastructure in Unity. Upon the implementation, it should be tested and verified on NICO using various tasks with different complexities. ADR will be evaluated in accordance with the most popular reinforcement learning techniques and the effectiveness for various scenarios will be measured with aim to explore the usability range and advantages of ADR.

## Progress Checklist

### Core Setup & Research
-  Studied introductory reinforcement learning materials (Sutton & Barto, relevant papers)
-  Finished Unity ML-Agents tutorials (e.g. Hummingbirds)
-  Chosen Soft Actor-Critic (SAC) as the RL algorithm (based on ML-Agents implementation)
-  Studied Chapter 3–4.6 of Sutton & Barto RL textbook
-  Identified candidate paper(s) for Active Domain Randomization

###  Unity & Simulation Setup
-  Set up Unity project for virtual twin of NICO
-  Defined NICO's movement control pipeline (Unity settings + RL output interpretation)
-  Trained NICO in Unity with 10 parallel instances (stabilizes after ~15 mins)

###  Experiments & Tools
-  Established reproducible training setup for later comparison
-  Training data is being logged for future benchmarking

###  Repo & Collaboration
-  GitHub repository created
-  Initial trained models prepared for upload

---

##  Upcoming Tasks

-  Read Chapter 5 of the RL textbook
-  Finalize baseline model and document all logging formats
-  Upload trained models to the GitHub repository
-  Wait for server availability to scale experiments (GPU cluster access pending)
-  Design ADR pipeline and test parameters' impact
-  Create first baseline RL training setup
-  Identified logging structure for training sessions (data collected for comparison)
  

---
