# Godot Animated Box Container Plugin C# Ver

![Godot v4.x](https://img.shields.io/badge/Godot-v4.x-%23478cbf?logo=godot-engine&logoColor=white&style=flat-square)

This plugin will animate layout direction change in container.

Due to behavior differences related to `UpdateMinimumSize` in C# and `update_minimum_size` in GDScript, this plugin will not be make available in GDScript.

**Important Notes: ** Please compile and reload the project first after enabling this plugin or before exporting the projecrt. We use hacks to bypass an C# plugin initization error in Godot.

Please submit any issues you found. Contribution and suggestions are warmly welcomed.

## Setting Recommendation

It is recommended not to use `Fill` size flag for both vertical and horizontal directions for children under `Animated Box Container`. Using it will have incorrect animation set for all children, causing unexpected behaviors.

Using `Animated Box Container` together with [Animated Panel Container](https://github.com/GramineaGroup/animated_panel_container_cs) is recommended as we have a special variable in `Animated Panel Container` for `Animated Box Container` to know the final size of any `Animated Panel Container` children.

## Demo

### Changing Layout Direction Like a Normal Box Container


https://github.com/user-attachments/assets/5f52a7bf-047e-4cbf-b406-63d6782f0674


### Changing Layout Direction but with Animation


https://github.com/user-attachments/assets/09a051e7-51b9-43ad-a26b-f022f9696270


### Integrated with `Animated Panel Container` to Get Scaling Animation


https://github.com/user-attachments/assets/3dd38ba8-d583-4ee8-8019-02139b501268


### Behavior Difference
It seems that Godot will call `Container.update_minimum_size` in GDScript for custom containers while the same behavior isn't observed in C#.
At the same time, GDScript doesn't allow us to overwrite non-virtual built-in function, which make it impossible to achieve the same animation.

Logically the same code, but behaves differently.

https://github.com/user-attachments/assets/7ff5f1d7-7f01-4964-9a56-c70320a2ad34




https://github.com/user-attachments/assets/fd88ebeb-8b83-4588-aada-8a396df1a3d5

