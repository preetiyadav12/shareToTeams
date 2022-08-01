const { BlobServiceClient } = require("@azure/storage-blob");
require("dotenv").config();

async function main() {
  console.log("Uploading bundled package to Azure Storage...");
  const containerName = process.env.CONTAINER;
  let azureStorageConnString = process.env.AZURE_STORAGE_CONNECTION;

  if (!azureStorageConnString) {
    const azureStorageAccountName = process.env.AZURE_STORAGE_ACCOUNTNAME;
    const azureStorageAccountKey = process.env.AZURE_STORAGE_ACCOUNTKEY;
  
    if (!azureStorageAccountName) {
      throw Error("Azure Storage Account Name string not found");
    }
    if (!azureStorageAccountKey) {
      throw Error("Azure Storage Account Key string not found");
    }
  
    // Construct Azure Storage Connection string
    azureStorageConnString = `DefaultEndpointsProtocol=https;AccountName=${azureStorageAccountName};AccountKey=${azureStorageAccountKey};EndpointSuffix=core.windows.net`;
  
  }

  // Get the list of files to upload
  const filesToUpload = process.env.FILES_TO_UPLOAD.split(",");
  if (!filesToUpload || filesToUpload.length === 0) {
    throw Error("No files found to upload");
  }

  console.log(`Connection String: ${azureStorageConnString}`)

  // Create the BlobServiceClient object which will be used to create a container client
  const blobServiceClient = BlobServiceClient.fromConnectionString(azureStorageConnString);

  console.log(`Creating container ${containerName}...`);

  // Get a reference to a container
  const containerClient = blobServiceClient.getContainerClient(containerName);
  // Create the container
  await containerClient.createIfNotExists({ access: "container" });
  console.log(`Container ${containerName} was created successfully. `);

  const getBlobName = (originalName) => {
    const fileParts = originalName.split("/");
    return fileParts[fileParts.length - 1];
  };

  //Upload all files into Azure storage
  filesToUpload.map(async (fileName) => {
    const blobName = getBlobName(fileName),
      blockBlobClient = containerClient.getBlockBlobClient(blobName);

    let options = {};
    if (blobName.includes("html")) {
      options = {
        blobHTTPHeaders: {
          blobContentType: "text/html",
        },
      };
    } else if (blobName.includes("js")) {
      options = {
        blobHTTPHeaders: {
          blobContentType: "text/javascript",
        },
      };
    } else if (blobName.includes("css")) {
      options = {
        blobHTTPHeaders: {
          blobContentType: "text/css",
        },
      };
    }

    console.log(`Uploading ${fileName}...`);

    await blockBlobClient.uploadFile(fileName, options);
    console.log(
      `${fileName} was successfully uploaded to Azure Storage at ${containerName}/${blobName}`,
    );
  });
}

main()
  .then(() => console.log("Done"))
  .catch((ex) => console.log(ex.message));
