using Microsoft.VisualBasic;
using System;
using System.Net.Security;

namespace StackOverFlow
{
    // Interface///
    public interface Votable
    {
        User GetOwner();
        void AddVote(Vote vote);
    }
    public interface Commentable
    {
        void AddComment(Comment comment);
    }

    // Enums  ///
    public enum VoteType
    {
        Upvote = +2,
        DownVote = -2,
        AcceptedAnswer = +10
    }


    // Classes///
    public class Vote
    {
        public User VotedBy;
        public VoteType VoteType;
        public Vote(VoteType type, User user)
        {
            this.VoteType = type;
            this.VotedBy = user;
        }
    }
    public class Comment
    {
        public string Text;
        public User CommentedBy;
        public Comment(string text, User commentBy)
        {
            this.Text = text;
            this.CommentedBy = commentBy;
        }

    }
    public class Question : Votable, Commentable
    {
        public int Id;
        public string Title;
        public string Description;
        public User PostedBy;
        public DateTime CreatedAt;
        public readonly List<Comment> Comments = new();
        public readonly List<Vote> votes = new();

        public Question(int id, string title, string description, User user)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.PostedBy = user;
            this.CreatedAt = DateTime.Now;
        }
        public User GetOwner()
        {
            return PostedBy;
        }

        // Voteable
        public void AddVote(Vote vote)
        {
            votes.Add(vote);
        }

        // Commentable
        public void AddComment(Comment comment)
        {
            Comments.Add(comment);
        }

    }
    public class Answer: Votable, Commentable
    {
        public int id;
        public string Description;
        public User PostedBy;
        public Question Questions;
        public DateTime CreatedAt;
        public readonly List<Comment> Comments = new();
        private readonly List<Vote> votes = new();
        public bool isAccepted;

        public Answer(int id, string description, User user, Question question)
        {
            this.id = id;
            this.Description = description;
            this.PostedBy = user;
            this.Questions = question;
            this.CreatedAt = DateTime.Now;
            this.isAccepted = false;
        }

        // Votable
        public User GetOwner()
        {
            return this.PostedBy;
        }
        public void AddVote(Vote vote)
        {
            this.votes.Add(vote);
        }
        public void AddComment(Comment comment)
        {
            Comments.Add(comment);
        }
        public void MarkAccepted()
        {
            this.isAccepted = true;
        }
    }

    public class User
    {
        public int Id { get; }
        public string Name { get; }
        public string Email { get; }
        public int Reputation { get; private set; }
        public User(int id, string name, string email)
        {
            this.Id = id;
            this.Name  = name;
            this.Email = email;
            this.Reputation = 0;
        }
        public void IncreaseReputation(int points)
        {
            this.Reputation += points;
        }
        public void DecreaseReputation(int points)
        {
            this.Reputation -= points;
        }
    }
    
    // Services ///
    public class ReputationService
    {
        public void HandleUpvote(User owner)
        {
            owner.IncreaseReputation((int)VoteType.Upvote);
        }
        public void HandleDownvote(User owner)
        {
            owner.DecreaseReputation((int)VoteType.DownVote);
        }
        public void HandleAcceptedAnswer(User owner)
        {
            owner.IncreaseReputation((int)VoteType.AcceptedAnswer);
        }
    }
    public class VoteServce
    {
        private readonly ReputationService _reputationService;
        public VoteServce(ReputationService reputation)
        {
            this._reputationService = reputation;
        }
        public void Upvote(Votable target, User PostedBy)
        {
            if(target.GetOwner() == PostedBy)
            {
                throw new Exception("Cannow upvote own post..");
            }
            Vote vote = new Vote(VoteType.Upvote, PostedBy);
            target.AddVote(vote);
            _reputationService.HandleUpvote(target.GetOwner());
        }
        public void Downvote(Votable target, User PostedBy)
        {
            if (target.GetOwner() == PostedBy)
                throw new Exception("Cannot downvote own posts...");

            Vote vote = new Vote(VoteType.DownVote, PostedBy);  
            target.AddVote(vote);
            _reputationService.HandleDownvote(target.GetOwner());   
        }
    }
   public class CommentService
    {
        public void AddComment(Commentable target, string text, User commentedBy)
        {
            Comment comment = new Comment(text, commentedBy); ;
            target.AddComment(comment); 
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            User alice = new User(1, "Alice", "Alice@gmail.com");
            User bob = new User(2, "Bob", "Bob@gmail.com");

            Question q = new Question(1, "What is LLD", "Explain about LLD.", alice);
            Answer a = new Answer(2, "LLD is low level design.", bob, q);

            VoteServce voteService = new VoteServce(new ReputationService());
            CommentService comment = new CommentService();

            voteService.Upvote(q, bob);
            comment.AddComment(a, "Good Answer", alice);


        }
    }
}