THE PLAN
=================================================
This is my personal (RobertMeta) plans for OCGTN

* Fix licenses
* Lower barrier to entry
* Convert to Mono
* Remove Google login
* Create useful default game


Fix licenses
-------------------------------------------------
Currently, the license of OCTGN and the Skylabs pieces are both privately held licenses.  I am trying 
to convince the respective authors to convert it over to a true open source license. 
Hopefully we can get it all under OSL approved licenses.


Lower barrier to entry
-------------------------------------------------
In order to make it easier to get involved the project has been moved to github, it has been setup to 
build with the free editions of visual studio.  Those are steps in the right direction, but the long
term goal is to make it cross platform and lower the barrier to entry even more.  See: Convert to Mono


Convert to Mono
-------------------------------------------------
I would like to convert the entire project to Mono with cross-platform GUI libs like GTK+.  This would 
allow people to use it on every platform, as well as develop on it from any platform.  This would bring
new batches of developers and users.


Remove Google login
-------------------------------------------------
I believe that attaching OCTGN to any existing accounts on any services is probably a bad idea.  It 
requires a high degree of trust, and essentially is asking for access to peoples most important account
(ie: thier email, pictures, etc).  If the decision to keep an external login holds, I would recommend
going to a straight OpenID, rather than trying to support any specific service directly.


Create useful default game
-------------------------------------------------
We should include standard playing cards as the default set that works with the game, currently some 
show-stopper bugs with mutual shared card access.  By having a default useful game, any questionable 
standing would be non-existant.
