from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor
from reportlab.lib.utils import ImageReader
from pathlib import Path
import textwrap
root=Path(__file__).resolve().parents[1]
images=root/'docs/images'
out=root/'docs/EQ Legends Achievement Companion - Easy Guide.pdf'
c=canvas.Canvas(str(out),pagesize=(960,780));c.setTitle('EQ Legends Achievement Companion - App Guide')
navy=HexColor('#101824');gold=HexColor('#A26D08');blue=HexColor('#235C9C')
page=0
def start(title,sub):
 global page
 page+=1;c.bookmarkPage('page'+str(page));c.addOutlineEntry(title,'page'+str(page),level=0);c.setFillColor(navy);c.rect(0,700,960,80,fill=1,stroke=0);c.setFillColor(HexColor('#F3C45D'));c.setFont('Helvetica-Bold',25);c.drawString(40,737,title);c.setFillColor(HexColor('#FFFFFF'));c.setFont('Helvetica',12);c.drawString(40,714,sub)
 c.setFillColor(blue);c.setFont('Helvetica',10);c.drawString(40,22,'EQ Legends Achievement Companion | v2026.09.17.1');c.drawRightString(920,22,str(page))
def para(text,x,y,width=110,size=14):
 c.setFillColor(navy);c.setFont('Helvetica',size)
 for line in textwrap.wrap(text,width):c.drawString(x,y,line);y-=size*1.45
 return y-10
def pic(name,x,y,w,h,crop=None):
 im=ImageReader(str(images/name));iw,ih=im.getSize();left,top,cw,ch=crop or (0,0,iw,ih);scale=min(w/cw,h/ch);dw=cw*scale;dh=ch*scale
 c.saveState();p=c.beginPath();p.rect(x,y,dw,dh);c.clipPath(p,stroke=0,fill=0);c.drawImage(im,x-left*scale,y-(ih-top-ch)*scale,width=iw*scale,height=ih*scale,mask='auto');c.restoreState()
def end():c.showPage()
start('App Guide: contents','Start here, or click a topic to jump directly to its page.')
topics=[('See the Companion in action',2),('Create your combat log and achievement export',3),('Connect your character and first-run setup',4),('Choose popups and sound',5),('Search and filter achievements',6),('Arrange your tracker',7),('Choose a farming location',8),('Understand live progress and completion',9),('Update, download and troubleshoot',10),('Add mob names and read Banestrike milestones',11),('Estimate time to completion',12)]
for i,(title,number) in enumerate(topics):
 y=650-i*43;c.setFillColor(blue);c.setFont('Helvetica',16);c.drawString(48,y,title);c.drawRightString(900,y,str(number));c.linkRect('', 'page'+str(number),(40,y-8,920,y+21),relative=0,thickness=0)
para('Use the PDF bookmarks or this clickable contents page to navigate. Screenshots show real gameplay; some controls may look slightly different in the current release.',40,135,width=105,size=14)
para('New player? Follow pages 3 and 4 first. Returning player? See pages 11 and 12 for mob names and time estimates.',40,75,width=105,size=14)
end()
start('Your achievements, beside your game','A quick illustrated guide. Your export is the confirmed baseline; live progress is an estimate.')
para('Start with the game-file setup and character connection pages. Then pick a zone, choose an achievement, and keep the tracker beside your game.',40,670)
pic('gameplay.png',40,75,880,530)
c.setFillColor(blue);c.setFont('Helvetica-Bold',13);c.drawString(40,50,'Watch the gameplay demonstration on YouTube');c.linkURL('https://www.youtube.com/watch?v=4GrrluSXVvk',(40,45,450,66),relative=0);end()
start('First: create your two game files','Do these steps while logged into the character you want to track.')
para('Combat log: click the game chat input, type /log on, and press Enter. Check for the logging-enabled message. Leave logging on while playing.',40,669,width=112,size=14)
para('Achievement export: open Inventory, click Achiev. (arrow 1), then Output To File (arrow 2). Include all categories and both completed and incomplete achievements when export options are offered.',40,608,width=112,size=14)
pic('export-achievements.png',140,163,680,405)
para('The export is a separate file from the combat log. Click Output To File again periodically to refresh confirmed progress. /log on alone does not export achievements.',40,132,width=118,size=13)
para('Next, open the Companion gear, choose the game folder, and click Find characters. Select your character and check that both files show connected. Enable live updates for new kills.',40,79,width=118,size=13)
end()
start('1. Connect your character','Open the clockwork gear. Settings stays inside the app and keeps your chosen theme.')
pic('settings.png',40,68,365,600,crop=(0,35,450,615))
y=665
for text in ['1. On first run, follow the Setup guide: choose the game folder, turn on logging and export achievements. You can reopen it from Settings.', '2. Pick your character and server. When both files are connected, click Start tracking. In Settings, Find characters scans the chosen folder again.', '3. Green checkmarks mean files were found. Red crosses mean a file is missing. The label tells you which one.', '4. Export achievements in game, including all categories and states. For a combat log, use /log on in game.', '5. Enable live updates to watch new log lines. Older kills are not replayed when monitoring starts.', 'Need another file? Expand Advanced file settings. Your game files are read-only.']:
 y=para(text,445,y,53,15)
end()
start('2. Choose popups and sound','Expand Popups & sound. Switches save immediately; release the volume slider to save it.')
pic('settings.png',40,280,390,365,crop=(0,275,450,198))
y=655
for text in ['Show popups and Play sounds are independent. You can use silent popups, sounds without popups, or neither.', 'Live updates keeps working when notifications are off.', 'Completion tune + small fireworks controls completion effects. The Play sounds switch still controls whether audio plays.', 'Use Test popup or Test completion to preview the effect without changing progress.', 'The speaker icon on a popup toggles sound and remembers your choice.']:
 y=para(text,470,y,49,15)
pic('popups.png',55,58,850,215);end()
start('3. Find useful achievements','Show filters to narrow the tiles. Expanded filters push the tiles down; Hide filters restores space.')
para('Choose Focus, Category, State, Farming region or Mob location. Category, region and zone dropdowns let you search their choices and check several at once.',40,670)
pic('zone-filter.png',40,92,460,485,crop=(0,245,895,710))
y=550
for text in ['Type into the search box inside a dropdown to find choices. Searching does not clear existing selections.', 'Use the main search field for achievement names, mobs, requirements and research notes.', 'Zone optimized Results temporarily shows location options matching your filters. Turn it off to return to your preferred saved locations.', 'Filters are remembered when you reopen. Reset filters clears the current filter selections.', 'The screenshot shows the zone checklist; the current release also has a search box above its choices.']:
 y=para(text,535,y,40,14)
end()
start('4. Arrange your tracker','Gold shows the selected Focus. Blue shows Overall. Expand Achievement summary for a short explanation.')
para('Auto shuffle now sits beside Achievement summary. On: recently progressed achievements move first. Off: drag a tile onto another tile to arrange your own order. Manual order is saved per character.',40,670)
pic('wide-tracker.png',40,135,880,440,crop=(0,335,1523,760))
para('Make the window narrow for one column. Click a tile to investigate; click it again or use Shrink to close the side panel. Pin keeps the app above other windows.',40,105)
para('Opacity applies when you leave the app; it becomes fully visible while you use it. The lightbulb switches light/dark mode.',40,57,size=12)
end()
start('5. Choose a farming location','The research panel holds the details. Your game export is never edited.')
para('Check Use in table beside your preferred location, then Save research. Research edits still need this button; automatic saving applies to Settings switches.',40,670)
pic('research.png',40,85,880,510,crop=(0,190,1504,800))
para('Personal notes stay in the panel and do not change the main table. Candidate locations and race variants may need in-game verification.',40,56,size=12)
end()
start('6. Understand live progress','Several achievements can progress from one kill. Up to three popups are visible; extras wait.')
pic('popups.png',70,280,820,390)
y=250
for text in ['Popups slide down and fade in. Progress popups stay eight seconds; completion popups stay thirteen seconds, with a gold border and a longer victory tune. When one expires, it fades out and the others slide up.', 'Your own kills and recognized current-pet kills can produce estimates. Generic bandit names do not identify a race, so the app does not guess their race credit.', 'Periodically make a fresh achievement export. The app detects it and updates the confirmed counts; you can also use Load / Refresh under Advanced file settings.', 'A popup can appear for an achievement hidden by your filters. It will not override your filters to insert that tile.']:
 y=para(text,40,y,110,14)
end()
start('7. Update and get help','The app checks GitHub automatically. You decide when to install an available update.')
y=660
for text in ['1. Look at the bottom-left version status. A gold Start update button appears when a newer version is available.', '2. Click the information icon for installed-version patch notes. Click Start update to download and verify the new executable.', '3. Confirm installation. The Companion closes, replaces its executable, and reopens. Your saved data remains in place.', 'For a fresh install or the latest illustrated guide, download the release ZIP and extract the whole folder. The executable updater replaces the app only; it does not refresh the PDF.', 'More > App Guide opens this PDF when installed. The ZIP also includes a short text guide.', 'A kill not counted? Check the selected character, connected log, Enable live updates, your filters, and whether the achievement is already complete. Compare a fresh export before deciding the game gave no credit.']:
 y=para(text,40,y,100,16)
c.setFillColor(blue);c.setFont('Helvetica-Bold',15);c.drawString(40,110,'Downloads and release notes');c.linkURL('https://github.com/Kaurus2/eq-legends-achievement-companion/releases',(40,105,500,126),relative=0);c.drawString(40,78,'Watch the gameplay demonstration');c.linkURL('https://www.youtube.com/watch?v=4GrrluSXVvk',(40,73,500,94),relative=0)
end()
start('8. Mob names and Banestrike milestones','Save your own mob names and see what remains for each Banestrike rank.')
y=655
for text in ['Add a mob name: click an achievement tile and find Mob names in the detail panel. Enter one name per line, then click + or press Ctrl+Enter to add them together.', 'Names save immediately. Use the remove button beside a name to delete it. Duplicate names are blocked, and each achievement keeps its own list.', 'Saved names also match future kills for that achievement. This works for incomplete Slayer achievements with one remaining numeric kill counter. Add names you have confirmed count in game.', 'Your list is stored locally in data/mob-names.json. Keep your data folder when updating. Lists are not uploaded automatically.', 'Choose BANESTRIKE focus to see three separate progress segments: Progressive, Highly Decorated, and A Force of Nature. Each is a +1 rank milestone; completed milestones turn gold.', 'Remaining counts come from the required entries in your latest export. Optional entries are excluded, and search or zone filters do not change those totals.', 'The main table now shows achievement, zone, target instructions, remaining count and progress. Open the detail panel for routes, faction requirements and specific unresolved questions.']:
 y=para(text,40,y,105,15)
end()
start('9. Estimate time to completion','The estimate sits on the tile header, between the achievement name and remaining count.')
y=655
for text in ['A kill-count Slayer tile starts with Learning pace. After at least five qualifying kills over 30 seconds, it shows an estimate such as ~12 min left.', 'The estimate uses your kills from the last five minutes and divides the estimated remaining count by that pace. It changes as you speed up or slow down.', 'After 90 seconds without a qualifying kill, the tile shows Paused. When kills resume after that gap, it learns a new pace.', 'Open the achievement for the fuller breakdown: estimated kills left, kills per minute and estimated time. Hover over the tile estimate for the same detail.', 'Names you save in Mob names contribute to this estimate when the logged kill matches. Other players\' kills do not count; recognized kills from your current pet can count.', 'The estimate covers killing time. Travel, finding a camp, turn-ins and other setup are not included.', 'A fresh achievement export replaces estimates with confirmed game progress and starts a new pace sample. Reaching an estimated target is not proof of completion.', 'With State set to Incomplete, confirmed completed achievements leave the results. Choose All states or Complete to view finished achievements.']:
 y=para(text,40,y,105,15)
end();c.save();print(out)
