//-----------------------PROGRAMER LOGS---------------------------

using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Utils;
using System;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class RewriteURLHelper
    {
        public static string Error404 => "/404";
        public static string Error403 => "/403";
        public static string Error500 => "/500";
        public static string Login => "/Login";
        public static string Home => "/Home";
        public static string AuditLogs => "/Audit-logs";
        public static string Settings => "/Settings";
        public static string Countries => "/Countries";
        public static string Provinces => "/Provinces";
        public static string Files => "/Files";
        public static string Wards => "/Wards";
        public static string Permission => "/Permission";
        public static string TaskSchedules => "/TaskSchedules";
        public static string Users => "/Users";

        //Project
        public static string Projects => "/Projects";
        public static string ProjectDetail(Guid projectId)
        {
            return $"/Project/{SecurityUtilities.ProtectUrlParameter(projectId.ToString())}";
        }
        public static string ProjectTasks(Guid projectId)
        {
            return $"/Project/{SecurityUtilities.ProtectUrlParameter(projectId.ToString())}/Tasks";
        }
        public static string AddTask(Guid projectId)
        {
            return $"/Project/{SecurityUtilities.ProtectUrlParameter(projectId.ToString())}/Task/Add";
        }

        public static string TaskDetail(Guid taskId, Guid projectId)
        {
            return $"/Project/{SecurityUtilities.ProtectUrlParameter(projectId.ToString())}/Task/{SecurityUtilities.ProtectUrlParameter(taskId.ToString())}";
        }
        //
        public static string ViewUser(Guid userId)
        {
            return $"/Users?userId={SecurityUtilities.ProtectUrlParameter(userId.ToString())}";
        }
        public static string Profile => "/Profile";
        public static string Roles => "/Roles";
        public static string AddRole => "/Role/Add";
        public static string RoleDetail(Guid roleId)
        {
            return $"/Role/{SecurityUtilities.ProtectUrlParameter(roleId.ToString())}";
        }
        //-----------------------------------------
        public static string Classes => "/Classes";
        public static string Grades => "/Grades";
        public static string GradeDetail(Guid gradeId)
        {
            return $"/Grade/{SecurityUtilities.ProtectUrlParameter(gradeId.ToString())}";
        }


        public static string Competitions => "/Competitions";
        public static string CompetitionDetail(Guid competitionId)
        {
            return $"/Competition/{SecurityUtilities.ProtectUrlParameter(competitionId.ToString())}";
        }

        //public static string CompetitionRounds => "/CompetitionRounds";
        public static string CompetitionRoundDetail(Guid competitionRoundId)
        {
            return $"/CompetitionRound/{SecurityUtilities.ProtectUrlParameter(competitionRoundId.ToString())}";
        }

        public static string ContextIndex(Guid conpetition, Guid roundId)
        {
            return $"/contest-index/{conpetition.ToString()}/{roundId.ToString()}";
        }

        public static string JoinCompetition()
        {
            return $"/join-competition";
        }
        public static string EmailTemplates
        {
            get
            {
                return "/email-templates";
            }
        }
        public static string EmailTemplateNew
        {
            get
            {
                return "/email-template/add";
            }
        }
        public static string EmailTemplateDetail(Guid emailId)
        {
            return $"/email-template/{SecurityUtilities.ProtectUrlParameter(emailId.ToString())}";
        }
    }
}
