# Craft2Git

## What is it?
Craft2Git is a tool designed to assist in Minecraft addon (.MCPACK, .MCWORLD) development 
by making it easy to transfer them between a folder such as a git repository and the game directory.

## Screenshots
![GUI](/images/image_1.png)

# How to use

1.Enter your version control/backup path in one textbox and the path of your game's "com.mojang" 
folder into the other. If there are any valid packs in those directories, the program should 
display them in the listboxes.

2. To move packs between the two directories, select one and hit the "copy" button below it. 
You should see the pack appear in the other window if it did not already exist there. The 
program will overwrite an existing pack of the same file name.


Additionally, the program should automatically detect changes such as manual deletion of a pack, but 
the refresh button is there just in case.

You can set default paths by navigating the "Preferences" menu near the top.
