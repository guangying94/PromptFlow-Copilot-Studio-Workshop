# Azure Machine Learning Service - PromptFlow Workshop
This repository aims to provide step by step to guide developer to develop end to end use cases for RAG pattern.

## What we will build
This sample is based on popular generative AI application pattern, retrival augmented generation (RAG), while providing the flexibility to refine the system prompt for more relevant response. In this sample, we will build a chatbot that answer questions based on standard of procedure document for corporate security in ficticious company called Contoso. User can talk to this chatbot via Microsoft Teams or M365 copilot in future.

## Pre-requisite
1. Azure subscription
2. Access to Azure Open AI / Open AI
3. (Optional, but highly recommended) Copilot Studio (Power Virtual Agent)
4. (Optional) Microsoft Teams as interface to chatbot


## Step 0: Data Preparation (Optional)
Firstly, we will generate dummy data using Bing Chat. Navigate to **Dummy Data Preparation** folder for the prompts and get sample data.

## Step 1: Deploy Necessary Azure Resources
To-do (Bicep) to deploy:
1. Azure Open AI
2. Azure AI Search
3. Azure Functions
4. Azure Machine Learning Services
5. Azure Storage Account
6. Azure AI Content Safety

## Step 2: Prepare Seach Indexer
In this example, we will use Azure Storage as the file repository to store document, and leverage Azure AI Search to index document and serves as a vector DB. To achive this, firstly, we need to have a pipeline that perform optical character recognition (OCR) on the documents to extract the content, then index them in Azure AI Search.

A simple way to achieve this is to leverage Azure AI Studio.