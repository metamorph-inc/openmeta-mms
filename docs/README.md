## Getting Started

To get started with this, install doxygen: http://www.stack.nl/~dimitri/doxygen/download.html

Then run `python build.py`

## Document Structure
Each folder should comprise a chapter.

The first document in each chapter folder should begin with the title of the chapter, marked as a highest-level heading:

	Chapter 1:  META Tool Introduction {#meta-tool-introduction}
    ==================================

Usually this heading will be followed by some text that introduces the chapter.

To indicate a subsection of a document, use two hash signs. Subsections will appear in the document navigation hierarchy under the parent chapter.
		
	## Subsection 1
	Here's content that will appear in the subsection.

To indicate a sub-subsection, use three has signs. Sub-subsections will appear in the document navigation hierarchy.

	### SubSubsection 1
	Some content for the sub-subsection

Further subsections should be indicated using four or more hash signs. These will not be included in the navigation hierarchy.

## Markdown Flavor
Doxygen has its own Github-like Markdown flavor. For details, read their **[documentation](http://www.stack.nl/~dimitri/doxygen/manual/markdown.html)**.


## Formatting Guidelines
Here are some guidelines for formatting text in these documents.

### Example Snippet Using Correct formatting
Next, let's add a couple of _alternatives_ to this _alternative design container_. To add an alternative:

1. Right-click the ***r\_\_100\_Ohm*** component in the **GME Browser**
2. Select **Copy**
3. Right-click on a blank area of the open canvas
4. Select **Paste Special -> As Reference**
5. Connect the ports of this new component instance to the external ports in the same way that it's been done for ***r\_\_1k\_Ohm***

### Tool or GME GUI Element
**Design Space Refactorer**

**Paste Special -> As Reference**

### Element in the user model
***SimpleLEDCircuit***

***MaxCurrentTest***

### CyPhy Model Class
_TestBench_

_Component_

_DesignSpace_

### Concepts
_alternatives_


## Documentation Practices
Added 8/10/15, tthomas@metamorphsoftware.com

### Copy and Paste
Use copy and paste as separate words, conjegating each of them separately, and spell out the word 'and.'
_"I like copying and pasting content to expedite my work." or "I don't like the section that you copied and pasted there."_

### Clicking
Use a hyphen when writing left-, right-, and double-click.  Capitalize only the first word if it begins a sentence.
_"... then right-click on ..." And "Double-click the file ..."_

### Hot-keys
Capitalized each key or modifier.  Use a single dash ('-') without spacing to separate multiple keystrokes.  If multiple modifiers keys are used, quote them in the following order: Ctrl, Alt, then Shift.  Place the commands in parentheses when they are used inline.
_"You can undo a mistake with the (Ctrl-Z) shortcut." Or "Press Ctrl-Shift-N to open a new window."_

### Filenames
File names and file extentions should be written as inline code snippets.
"Double-click on `LED_Tutorial.xme`." Or "Unzip the `.zip` file to ..."

### Notes
A note in the text should be a separate paragraph that begins with "Note:" and is all italisized.
"_Note: This only works if you are in Command Mode._"

### Other Guidelines
* Use the first-person plural active voice when possible. "We will begin by ..."
* Don't necessarily introduce every concept the first time you encounter it as to not overwhelm the user.
* Use a single space between sentences.
* Use italics to indicate emphasis.