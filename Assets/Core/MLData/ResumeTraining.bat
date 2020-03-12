@echo off
mlagents-learn ./config/trainer_config.yaml --env=HighwayMan --run-id=AgentDriver --train --load
pause