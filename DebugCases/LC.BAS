' Several DECLARE SUB lines, two of which get split with line continuations.

DECLARE SUB GameOver (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, trex AS TRexType)
DECLARE SUB HandleKeys (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%, KeyStates%())
DECLARE SUB InitializeCloud (cloud AS CloudType)
DECLARE SUB InitializeCollisionBox (collisionbox AS CollisionBoxType, XPos%, YPos%, widthval%, heightval%)
DECLARE SUB InitializeDistanceMeter (distancemeter AS DistanceMeterType)
DECLARE SUB InitializeGame (runner AS RunnerType, loopstate AS LoopStateType, horizonline AS HorizonLineType, clouds() AS CloudType, nightmode AS NightModeType, obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, distancemeter  _
AS DistanceMeterType, trex AS TRexType)
DECLARE SUB InitializeHorizonLine (horizonline AS HorizonLineType)

