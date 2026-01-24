                                     ** Requirements:**

1️⃣ Who are the users?

**Users can be:**
Guests (read-only),
Registered Users

**Only registered users can:**
1. Ask questions,
2. Answer questions,
3. Comment,
4. Vote,
(No admin/moderator logic needed for now.)

2️⃣ What actions can users take?
**Core actions (IN SCOPE):**
1. Ask a question,
2. Answer a question,
**Comment on:**
1. Questions,
2. Answers
**Upvote / Downvote:**
1. Questions,
2. Answers
**Out of scope:**
1. Editing posts,
2. Closing questions,
3. Flagging,
4. Bounties,
5. Badges

**3️⃣ What is in scope vs out of scope?**
✅ **In Scope:**
1. Posting questions,
2. Posting answers,
3. Voting,
4. Commenting,
5. Basic reputation system,
6. Accepting an answer

❌ **Out of Scope:**
1. Search,
2. Tags hierarchy,
3. Notifications,
4. Moderation tools,
5. Pagination,
6. Performance optimization

**4️⃣ Reputation — how should it work?**
Reputation is user-based,
Simple rules:
1. +10 for upvote on question/answer,
2. −2 for downvote,
3. +15 for accepted answer,
Reputation updates immediately


                    **Entities**
**Entities:**
1. User
2. Question
3. Answer
4. Comment
5. Vote
  Reputation is maintained as a property of User and updated based on votes and accepted answers.

1️⃣ User ✅
Represents a registered user.
1. Asks questions,
2. Answers questions,
3. Votes,
4. Earns reputation

User is the owner of reputation.

2️⃣ Question ✅
1. Created by a User
2. Has Answers
3. Has Comments
4. Can be voted on
5. Can have an accepted Answer
Core entity.

3️⃣ Answer ✅
1. Belongs to a Question
2. Created by a User
3. Can be voted on
4. Can be accepted
Another core entity.

4️⃣ Comment ✅
1. Can belong to:
2. Question OR
3. Answer
4. Created by a User
Important modeling point (polymorphic association).

5️⃣ Vote ✅

1. Represents a voting action
2. Has:
  a.VoteType (Up / Down)
  b. Target (Question or Answer)
  c. VotedBy (User)
3.Vote as an entity avoids:
  a. Multiple votes by same user
  b. Incorrect reputation calculation

❌ Why Reputation Is NOT an Entity
Because:
1. It has no independent lifecycle
2. It cannot exist without User
3. It’s always derived from actions
4. Storing it separately adds unnecessary complexity


                                          **RelationShips**
1. User -> Question = Aggregation
2. User -> Answer = Aggregation
3. Question -> Answer = Composition
4. Question -> Comment = Composition
5. Answer -> Comment = Composition
6. User -> Vote = Aggregation
7. Question/Answer -> Vote = Aggregation

                      **Attribues**
   User:
- Id
- Name
- Email
- Reputation

Question:
- Id
- Title
- Description
- PostedBy (User)
- CreatedAt
- List of Answer
- List of Comment
- List of Vote
- AcceptedAnswerId (optional)

Answer:
- Id
- Description
- PostedBy (User)
- Question
- CreatedAt
- List of Comment
- List of Vote
- IsAccepted

Comment:
- Id
- Text
- PostedBy (User)
- CreatedAt

Vote:
- Id
- VoteType (Upvote / Downvote)
- VotedBy (User)
- CreatedAt

                        **Rules for Upvotes, downvotes..**
1. Upvote Question
- Only registered users can upvote
- A user cannot upvote their own question
- Each user can upvote a question only once
- The question owner's reputation increases (e.g., +10)

2. Downvote Answer
- Only registered users can downvote
- A user cannot downvote their own answer
- Each user can downvote an answer only once
- The answer owner's reputation decreases (e.g., −2)
- The user who downvotes also loses some reputation (e.g., −1)

3. Accept Answer
- Only the question owner can accept an answer
- Only one answer can be accepted per question
- The accepted answer is marked as accepted
- The answer owner's reputation increases (e.g., +15)

          **Method for Entity**
Question:
- AddAnswer(Answer answer)
- AddComment(Comment comment)
- MarkAnswerAsAccepted(Answer answer)
Answer:
- AddComment(Comment comment)
- MarkAsAccepted()
Vote:
- ApplyVote()

| Your Method  | Correct Place      |
| ------------ | ------------------ |
| PostQuestion | QuestionService    |
| GetQuestion  | QuestionRepository |
| VoteQuestion | VoteService        |
| PostAnswer   | AnswerService      |
| GetAnswer    | AnswerRepository   |
| VoteAnswer   | VoteService        |


                  **VoteService**
-**interface Votable**
- getId()
- getOwner()
- addVote(Vote vote)
Both:
Question, Answer
implement Votable.

VoteService:
- upvote(Votable target, User votedBy)
- downvote(Votable target, User votedBy)

**What VoteService ACTUALLY Does (Responsibility)**
-**Validates**:
-User cannot vote own post
-User cannot vote twice
-Creates a Vote
-Attaches Vote to target
-Triggers reputation update (via ReputationService)


                    **ReputationService**:
- handleUpvote(Votable target)
- handleDownvote(Votable target, User votedBy)
- handleAcceptedAnswer(Answer answer)

                  **CommentService**
-interface Commentable
- addComment(Comment comment)

CommentService:

1. addComment(Commentable target, String text, User commentedBy)
   - creates a comment and attaches it to the target
