# SerializeableGuid
## Description
This class is used to generate a Globally Unique Identifier and serialize it. Includes some util functions to turn it to a string or a shader-safe string.

## Class Details
### Attributes
- Serializable - Unity will serialize any serializeable fields in this class

### Fields
| Name           | Type   | Description                     |
| -------------- | ------ | ------------------------------- |
| guid           | Guid   | Generated GUID - not serialized |
| serializedGuid | byte[] | Generated GUID - serialized     |

