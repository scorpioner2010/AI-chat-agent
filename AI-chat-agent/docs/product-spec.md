# Product Spec

AI Chat Agent is a universal desktop chat assistant for analyzing profiles and conversations, preparing AI-generated replies, and helping the user manage conversations across multiple websites.

The system will support different websites through platform adapters. Each adapter will isolate website-specific behavior, selectors, browser interactions, and data mapping from the shared domain and application logic.

Core planned capabilities include profile and conversation analysis, a conversation state machine, AI-generated reply suggestions, compatibility scoring, and a shortlist for prioritizing promising conversations.

The WPF dashboard will provide the local desktop interface for reviewing profiles, conversation state, generated replies, scores, and shortlist entries.

Two operating modes are planned:

- Mock mode: runs against deterministic local fixtures for development, testing, and demos without website access.
- Browser mode: runs through website adapters and browser automation infrastructure when that integration is added later.

This document describes the base direction only. OpenAI integration, browser automation, SQLite persistence, domain entities, conversation logic, and UI redesign are intentionally out of scope for the initial architecture setup.
