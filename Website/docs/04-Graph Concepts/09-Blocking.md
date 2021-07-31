# Blocking

You can block/disable `ports` and `edges` to avoid data processing or transfer. Its main uses includes discarding unwanted data (for example, mesh parts that are not externally visible and hence could be excluded from the final ouput) or temporarily excluding parts of the graph for purposes of debugging.

Edges and ports can be blocked, making them act as they wouldn't exist. So:

* If you block output ports, the data is discarded.
* If a node is disabled, then it acts as if it would not exist. So the data from the output port will still send out data.
* If you block input ports, the data is discarded once it arrives there.


![](images/blockPorts.png)

## How to block

* **To block edges:** Right-click them and choose 'Disable' or Alt+Left mouse click it.
* **To block ports:** Click them and change the 'State' to 'Blocked' in the inspector menu or Alt+Left mouse click it.