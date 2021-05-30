texassemble -cube -w 512 -h 512 -f R8G8B8A8_UNORM -o SimpleSky.dds SimpleSky.png SimpleSky.png SimpleSkyTop.png SimpleSkyBottom.png SimpleSky.png SimpleSky.png 
texconv -pow2 -f BC1_UNORM SimpleSky.dds
PAUSE