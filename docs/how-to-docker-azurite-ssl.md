//https://github.com/FiloSottile/mkcert
mkcert -install
mkcert -key-file /Users/msiccdev/LocDev/azurite/127.0.0.1-key.pem -cert-file /Users/msiccdev/LocDev/azurite/127.0.0.1.pem 127.0.0.1

//https://github.com/Azure/Azurite/blob/main/README.md#https-setup
docker run -p 10000:10000 -v /Users/msiccdev/LocDev/azurite:/workspace -l /workspace mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0 --oauth basic --cert /workspace/127.0.0.1.pem --key /workspace/127.0.0.1-key.pem 