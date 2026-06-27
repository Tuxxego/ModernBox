//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using ai;
using ai.behaviours;

namespace ModernBox
{
    class NewJobs : MonoBehaviour
    {
		public static BehaviourTaskActorLibrary JetTasks = new BehaviourTaskActorLibrary();


        public static void init()
        {


            loadBeh();
            loadJobs();
        }

        private static void loadBeh()
        {

			
			BehaviourTaskActor JetBeh = new BehaviourTaskActor();
			JetBeh.id = "jet";
            JetBeh.fighting = true;
			JetBeh.addBeh(new BehFindRandomTile8Directions());
			JetBeh.addBeh(new BehGoToTileTarget());
			JetBeh.addBeh(new BehFightCheckEnemyIsOk());
			JetBeh.addBeh(new BehFindRandomTile8Directions());
			JetBeh.addBeh(new BehGoToTileTarget());
			JetBeh.addBeh(new BehFindRandomTile8Directions());
			JetBeh.addBeh(new BehGoToTileTarget());
			JetBeh.addBeh(new BehRestartTask());
            AssetManager.tasks_actor.add(JetBeh);
            JetTasks.add(JetBeh);




        }

        private static void loadJobs()
        {

			
			ActorJob JetJob = new ActorJob();
            JetJob.id = "jet_job";
            JetJob.addTask("jet");
            AssetManager.job_actor.add(JetJob);



        }



		 public static string getNextKingdomJobModern()
		 {
			 return "kingdom_modern";
		 }
		
		 public static string getNextKingdomJob()
		 {
			 return "kingdom";
		 }

		 public static string getNextCityJobModern()
		 {
			 return "city_modern";
		 }

		 public static string getNextCityJob()
		 {
			 return "city";
		 }
    }

}