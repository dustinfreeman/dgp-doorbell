-- Very bare bones Growl notification script for DGP Doorbell.
-- Jonathan Deber, based on Growl AppleScript starter code (http://growl.info/documentation/applescript-support)
-- version 1.0

-- Requires: - Mac OS X (what with it being AppleScript), tested on 10.6, should work on any recent vintage OS
--           - Mail.app as your email client, tested with 4.5
--           - Growl, tested with 1.2.2 (in theory should work with 1.3.x, but I haven't tried it yet)

-- Instllation: Copy this script into a known location (~/Library/Scripts would be traditional; it's possible that this directory does not exist in your ~/Library, at which point you can just create it (note that Lion by default hides ~/Library in the Finder, but you can still access it via the Terminal)).  Next, in Mail, go to Mail->Preferences and select the "Rules" tab.  Create a new rule for messages from doorbell@[domain] (filling in the domain as appropriate), and select "Run Applescript" as the action.  Click on "Choose...", and locate this script.  Save the rule, and you're done.

-- Configuration: By default, the notifications are sticky, so they don't disappear automatically.  You can change this (and other things, like audio options) in the Growl System Preference panel.

-- Todo:
--  - Better error handling / testing on more system configs
--  - Handling of clicks (i.e., opening the email or photo if the notification is clicked on)
--  - Image in notification?  Would it be readable?
--  - Support for browser-based Gmail rather than Mail.app (this might be challenging, since we need a way to launch an AppleScript)


-- Check if Growl is running
tell application "System Events"
	set isRunning to Â
		(count of (every process whose bundle identifier is "com.Growl.GrowlHelperApp")) > 0
end tell

if isRunning then
	tell application id "com.Growl.GrowlHelperApp"
		-- Make a list of all the notification types 
		-- that this script will ever send:
		set the allNotificationsList to Â
			{"Doorbell Notification"}
		
		-- Make a list of the notifications 
		-- that will be enabled by default.      
		-- Those not enabled by default can be enabled later 
		-- in the 'Applications' tab of the Growl preferences.
		set the enabledNotificationsList to Â
			{"Doorbell Notification"}
		
		-- Register our script with growl.
		-- You can optionally (as here) set a default icon 
		-- for this script's notifications.
		register as application Â
			"Doorbell Notifier" all notifications allNotificationsList Â
			default notifications enabledNotificationsList Â
			icon of application "Mail.app"
		
		--       Send a Notification...
		notify with name Â
			"Doorbell Notification" title "Doorbell Notification" description "Ding Dong." application name "Doorbell Notifier" sticky yes priority 1
	end tell
end if
