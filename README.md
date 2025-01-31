# Blackbird.io Contentful

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Contentful is a headless CMS that empowers businesses to create, manage, and deliver digital content across various platforms and devices. It is primarily used for efficient organization and distribution of content, offering features like content modeling, versioning and API-driven content delivery.

## Before setting up

Before you can connect you need to make sure that:

- You have a Contentful account with a space you want to connect to.
- Your Contentful account has the right permissions in the space. You can read more about space roles and permissions [here](https://www.contentful.com/help/space-roles-and-permissions/).
- You have your own [OAuth application](https://app.contentful.com/account/profile/developers/applications) created.

## Create OAuth application

1. Navigate to [OAuth applications](https://app.contentful.com/account/profile/developers/applications) page and click _New Application_.
2. Enter application _Name_ and _Description_. In _Redirect URI_ field specify `https://bridge.blackbird.io/api/ImplicitGrant`. This is the URL where Blackbird will receive the authorization code.
3. Under _Redirect URI_ select _Content management manage_.
4. Click _Create Application_.
5. After the application is created, _Client ID_ is generated. _Client ID_ is required to connect to Contentful via Blackbird.

## Connecting

1. Navigate to apps and search for Contentful.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My client'.
4. Fill in the _Client ID_ obtained earlier.
5. Fill in the _Space ID_ of the Contentful space you want to connect to. To find space ID follow [this instructions](https://www.contentful.com/help/find-space-id/).
6. Base URL: By default, set the base URL to https://api.contentful.com. If you are operating in the EU, you should change this to https://api.eu.contentful.com.
7. Click Authorize connection.
8. Follow the instructions that Contentful provides, authorizing Blackbird.io to act on your behalf.
9. When you return to Blackbird, confirm that the connection has appeared and the status is Connected.

![connecting](image/README/connecting.png)

## Working with translations

Follow [this guide](https://www.contentful.com/help/working-with-translations/) to be able to work with translations in Contentful.

## Actions

### Entries

- **Get locales** returns the default locales and a list of other locales, all in code form to easily use in conjunction with the convert operator.
- **Get entry's text/rich text field** returns the content of short text, long text or rich text field of the entry as a string.
- **Get text/rich text field as HTML file** returns the content of short text, long text or rich text field of the entry as an HTML file.
- **Set entry's text/rich text field** sets the content of short text, long text or rich text field of the entry from a string.
- **Set text/rich text field from HTML file** sets the content of short text, long text or rich text field of the entry from an HTML file. For short/long text only the text extracted from the file is put in the field. For rich text all HTML structure is preserved.
- **Get entry's number field** returns the entry's number field value.
- **Set entry's number field** sets the entry's number field value.
- **Get entry's boolean field** returns the entry's boolean field value.
- **Set entry's boolean field** sets the entry's boolean field value.
- **Get entry's media content** returns the identifier of the asset attached to the entry's media field.
- **Set entry's media field** sets the entry's media field with the specified asset.
- **Search entries** returns a list of entries. Optionally filter by content model, environment, tags and the last updated date.
- **Get entry** Get details of a specific entry
- **Add new entry** creates a new entry with the specified content model.
- **Find entry by field value** Given a field and a desired value for the field, the first matching entry will be returned.
- **Delete entry**.
- **Publish entry**.
- **Unpublish entry**.
- **List missing locales for a field** returns a list of missing translations for the specified field.
- **List missing locales for entry** returns a list of missing translations for the specified entry.
- **Get entry as HTML file** returns all localizable fields of the specified entry as HTML file (see more information below).
- **Update entry from HTML file** updates all localizable fields of specified entry .

**Get entry as HTML file** and **Update entry from HTML file** are intended to be used together in a typical translation flow: you can retrieve an entry's localizable fields as HTML file, put it into TMS, then retrieve a translated HTML file and put it back into Contentful's entry. **Update entry from HTML file** expects the same HTML structure as the structure of the file retrieved with **Get entry as HTML file**.

**Important note**: make sure your entry has fields with **localization enabled**. You have to explicitly set this property on each field (see images below).

![1707747998688](image/README/1707747998688.png)
![1707748006274](image/README/1707748006274.png)

The **Get entry as HTML file** action also lets you define if you want to recursively embed content (for translation) from linked entries.

There are 4 types of linked entries:
- Reference field types from the content model
- Hyperlinks that link to an entry in 'Rich text' fields
- Inline embedded entries in 'Rich text' fields
- Block embedded entries in 'Rich text' fields

In the action you are able to select exactly which type of linked entry you want to include in the exported HTML file. If you f.e. select 'Hyperlinks' and 'Inline embedded entries', we will recursively search through all 'Rich text' fields and fetch all the content of these embedded entries. For these embedded entries, we do the same thing and also get all hyperlinks and embedded inline entries, and so on.

> Note: you can also specify if you want to ignore the localization aspect of reference fields. If this optional input is true, and the 'Include referenced entries' is true, then all referenced entries will be included regardless of localization setting.

Finally, you can specify a list of Field IDs which will always be ignored and not added to the produced HTML file.

> For more information about CMS localization. Check out [this guide](https://docs.blackbird.io/guides/cms-contentful/).

### Assets

- **Get asset** returns title, description and a file attached to the asset.
- **Create and upload asset**.
- **Update asset file**.
- **Delete asset** Delete specified asset.
- **Publish asset**.
- **Unpublish asset**.
- **Is asset locale present** checks if specified file translation is present for the asset.
- **List missing locales for an asset** returns a list of all missing translations for the asset.
- **Add asset tag** Add a new tag to the specified asset
- **Remove asset tag** Remove a specific tag from the asset

### Content models

- **List all content models** returns all content models available in space.
- **Get content model** Get details of a specific content model.

### Tag
- **List tags** List all content tags in a space
- **Create tag** Create a new content tag
- **Get tag** Get details of a specific tag
- **Delete tag** Delete specific content tag
- **Add tag to entry** Add specific tag to an entry
- **Remove tag from entry** Remove specific tag from an entry

### Content types
- **List all content types** returns all content types available in space.

### Entry tasks

- **Search entry tasks** returns a list of tasks based on the specified filters.
- **Get entry task** returns details of a specific task.
- **Update entry task** updates an entry task with new details.

Note, to use the **Entry tasks** actions, you need to install the `Workflows` app (Developed by Contentful) in your Contentful space.

### Workflows

- **Get workflow** returns details of a specific workflow based on the workflow ID.
- **Get workflow definition** returns details of a specific workflow definition based on the workflow definition ID.
- **Update workflow step** move a workflow to a specific step.
- **Complete workflow** completes a workflow.
- **Cancel workflow** cancels a workflow.

## Events

- **On entry published** and **On asset published** are the most useful events. They are triggered when any entry/asset is published and could be the perfect trigger for sending the entry/asset for translation based on the missing translations (see example). You can optionally also filter these events by tags. If you do so, the event will only be triggered if all of the tags you input are present on the entry.
- **On entry task created** and **On entry task saved** are useful if you prefer to work using the 'Workflows' extension of Blackbird. You can use the extension to assign an entry to a workflow, creating a task. these events can even filter based on assigned User ID and task description. See the example below.
- **On workflow updated** triggers when a workflow updated or created. Also triggers when a workflow changes its step.
- **On workflow completed** triggers when a workflow is completed.

### Other events:

- **On entry created**
- **On entry saved**
- **On entry auto saved**
- **On entry unpublished**
- **On entry archived**
- **On entry unarchived**
- **On entry deleted**
- **On asset created**
- **On asset saved**
- **On asset auto saved**
- **On asset unpublished**
- **On asset archived**
- **On asset unarchived**
- **On asset deleted**

## Examples

![example](image/README/example.png)

In this example, whenever an entry is published we retrieve the localizable fields as an HTML file and fetch the missing translations. Then we create a new Phrase project with the missing locales as the target languages and create a new Phrase job for the file.

![example2](image/README/1727786768856.png)

In this example, whenever an entry is published we retrieve all localizable fields as an HTML file. We then send the fiel to DeepL for translation and immediatly update the translated content. Additionally, we are taking a glossary from XTM and use that in our DeepL translation.

![example3](image/README/1727786944492.png)

In this example we are using the 'Workflows' feature in Contentful. When a new task is created (we can addtionally filter on task body and assigned user) we will pull the entry related to this task as an HTML file, translate it with DeepL and update the translation in Contentful. Additionally, we update the status of the task to mark it as resolved.

![example4](image/README/workflow-example.png)

In this example we are also using the 'Workflows' feature in Contentful, but in this case we don't work with tasks. Instead, we are using the workflow itself. When a workflow changes its step we are checking if the step is 'Ready for translation'. If it is, we are pulling the entry as an HTML file, translating it with DeepL and updating the translation in Contentful. Then we are moving the workflow to the next step or if there is no next step, we are completing the workflow.

## HTML features

We add metadata to the HTML file to include `Entry ID` and `Field ID`. This metadata is used to update the entry content from the HTML file. These tags are used to identify the content in the `Contentful`, eliminating the need to store IDs elsewhere.

Example of how we include metadata in the HTML file:

```html
<html>
<head>
    <meta name="blackbird-entry-id" content="example-entry-id">
    <meta name="blackbird-field-id" content="example-field-id">
</head>
<body>
    <p>Toothbrush</p>
</body>
</html>
```

## Missing features

Most content related actions exist. However, in the future we can add actions for more field types. Let us know if you're interested!

If you want to have a more convenient experience managing your Contentful with Blackbird, consider cloning this app and modifying it so it aligns with your configuration.

## Feedback

Feedback to our implementation of Contentful is always very welcome. Reach out to us using the established channels or create an issue.

<!-- end docs -->
