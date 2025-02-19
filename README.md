# Webhook Rate-Limited Processor

## Overview
This is a .NET 4.8 console application that processes webhook requests efficiently while adhering to rate limits. It leverages **Hangfire** for background job management and **Polly** for resilience and retry policies.

## Features
- **Customizable Rate Limit:** Supports configurations like `"Allow 1000 requests every 5 minutes"`.
- **Rate-Limited Queuing:**  
  - When the rate limit is exceeded, webhook requests will be queued and processed after the limit refreshes.
  - If a webhook has not reached its rate limit, new content event requests will be processed almost immediately.
- **Sequential Processing:** Webhook requests are processed one after another in a single Hangfire background job.
- **Dedicated Hangfire Queue:** The background job operates on a separate Hangfire queue and does not consume workers from the main queue.

## Technologies Used
- **.NET Framework 4.8**
- **Hangfire** – Background job processing
- **Polly** – Resilience and transient fault handling

## Installation and Configuration

### Prerequisites
1. Ensure you have **.NET Framework 4.8** installed.
2. Install required dependencies using NuGet:
   ```powershell
   Install-Package Hangfire
   Install-Package Polly
