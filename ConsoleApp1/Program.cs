using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversityApp
{
    abstract class Person
    {
        public int Id { get; }
        public string Name { get; }
        public int Age { get; }
        public string Contact { get; }
        protected Person(int id, string name, int age, string contact) { Id = id; Name = name; Age = age; Contact = contact; }
        public abstract string GetInfoOneLine();
    }

    class Student : Person
    {
        private readonly HashSet<int> courseIds = new();
        public Student(int id, string name, int age, string contact) : base(id, name, age, contact) { }
        public void Enroll(int courseId) { if (!courseIds.Add(courseId)) throw new InvalidOperationException("Студент уже записан на курс."); }
        public void Unenroll(int courseId) => courseIds.Remove(courseId);
        public IEnumerable<int> Courses => courseIds;
        public override string GetInfoOneLine() => $"Студент #{Id}: {Name}, {Age} лет, контакт: {Contact}; Курсы: {(courseIds.Any() ? string.Join(",", courseIds) : "—")}";
    }

    class Teacher : Person
    {
        private readonly HashSet<int> courseIds = new();
        public Teacher(int id, string name, int age, string contact) : base(id, name, age, contact) { }
        public void AssignCourse(int courseId) => courseIds.Add(courseId);
        public void RemoveCourse(int courseId) => courseIds.Remove(courseId);
        public IEnumerable<int> Courses => courseIds;
        public override string GetInfoOneLine() => $"Преподаватель #{Id}: {Name}, {Age} лет, контакт: {Contact}; Курсы: {(courseIds.Any() ? string.Join(",", courseIds) : "—")}";
    }

    class Course
    {
        public int Id { get; }
        public string Title { get; }
        public int? TeacherId { get; private set; }
        private readonly HashSet<int> students = new();
        public Course(int id, string title) { Id = id; Title = title; }
        public void SetTeacher(int? tId) => TeacherId = tId;
        public void AddStudent(int sId) { if (!students.Add(sId)) throw new InvalidOperationException("Студент уже записан на курс."); }
        public void RemoveStudent(int sId) => students.Remove(sId);
        public IEnumerable<int> Students => students;
        public string GetInfoOneLine()
        {
            var teacher = TeacherId.HasValue ? TeacherId.Value.ToString() : "не назначен";
            var studs = students.Any() ? string.Join(",", students) : "—";
            return $"Курс #{Id}: {Title}; Преподаватель ID: {teacher}; Студентов: {students.Count()} (IDs: {studs})";
        }
    }

    class UniversitySystem
    {
        private readonly Dictionary<int, Student> students = new();
        private readonly Dictionary<int, Teacher> teachers = new();
        private readonly Dictionary<int, Course> courses = new();
        private int nextStudent = 1, nextTeacher = 1, nextCourse = 1;

        public int AddStudent(string name, int age, string contact) { var id = nextStudent++; students[id] = new Student(id, name, age, contact); return id; }
        public int AddTeacher(string name, int age, string contact) { var id = nextTeacher++; teachers[id] = new Teacher(id, name, age, contact); return id; }
        public int AddCourse(string title, int? teacherId = null)
        {
            var id = nextCourse++; var c = new Course(id, title);
            if (teacherId.HasValue) { if (!teachers.ContainsKey(teacherId.Value)) throw new KeyNotFoundException("Преподаватель не найден."); c.SetTeacher(teacherId.Value); teachers[teacherId.Value].AssignCourse(id); }
            courses[id] = c; return id;
        }

        public Student GetStudent(int id) => students.TryGetValue(id, out var s) ? s : throw new KeyNotFoundException("Студент не найден.");
        public Teacher GetTeacher(int id) => teachers.TryGetValue(id, out var t) ? t : throw new KeyNotFoundException("Преподаватель не найден.");
        public Course GetCourse(int id) => courses.TryGetValue(id, out var c) ? c : throw new KeyNotFoundException("Курс не найден.");

        public void RemoveStudent(int id)
        {
            if (!students.ContainsKey(id)) throw new KeyNotFoundException("Студент не найден.");
            foreach (var c in courses.Values) c.RemoveStudent(id);
            students.Remove(id);
        }

        public void RemoveTeacher(int id)
        {
            if (!teachers.ContainsKey(id)) throw new KeyNotFoundException("Преподаватель не найден.");
            foreach (var c in courses.Values.Where(x => x.TeacherId == id)) c.SetTeacher(null);
            teachers.Remove(id);
        }

        public void RemoveCourse(int id)
        {
            if (!courses.ContainsKey(id)) throw new KeyNotFoundException("Курс не найден.");
            foreach (var s in students.Values) s.Unenroll(id);
            var tId = courses[id].TeacherId;
            if (tId.HasValue && teachers.ContainsKey(tId.Value)) teachers[tId.Value].RemoveCourse(id);
            courses.Remove(id);
        }

        public void AssignTeacher(int courseId, int teacherId)
        {
            var c = GetCourse(courseId); var t = GetTeacher(teacherId);
            if (c.TeacherId.HasValue && teachers.ContainsKey(c.TeacherId.Value)) teachers[c.TeacherId.Value].RemoveCourse(courseId);
            c.SetTeacher(teacherId); t.AssignCourse(courseId);
        }

        public void EnrollStudent(int studentId, int courseId)
        {
            var s = GetStudent(studentId); var c = GetCourse(courseId);
            c.AddStudent(studentId); s.Enroll(courseId);
        }

        public void PrintAllDataOneLine()
        {
            Console.WriteLine("\n--- Все студенты ---");
            if (!students.Any()) Console.WriteLine("(Нет студентов)");
            else foreach (var s in students.Values) Console.WriteLine(s.GetInfoOneLine());

            Console.WriteLine("\n--- Все преподаватели ---");
            if (!teachers.Any()) Console.WriteLine("(Нет преподавателей)");
            else foreach (var t in teachers.Values) Console.WriteLine(t.GetInfoOneLine());

            Console.WriteLine("\n--- Все курсы ---");
            if (!courses.Any()) Console.WriteLine("(Нет курсов)");
            else foreach (var c in courses.Values) Console.WriteLine(c.GetInfoOneLine());
        }

        // Меню-выборы (возвращают ключ или -1)
        public int ChooseStudent()
        {
            if (!students.Any()) { Console.WriteLine("Студенты отсутствуют."); return -1; }
            var keys = students.Keys.ToList();
            for (int i = 0; i < keys.Count; i++) Console.WriteLine($"{i + 1}) {students[keys[i]].GetInfoOneLine()}");
            Console.WriteLine("0) Отмена");
            int sel = ReadIntBetween(0, keys.Count);
            return sel == 0 ? -1 : keys[sel - 1];
        }

        public int ChooseTeacher()
        {
            if (!teachers.Any()) { Console.WriteLine("Преподаватели отсутствуют."); return -1; }
            var keys = teachers.Keys.ToList();
            for (int i = 0; i < keys.Count; i++) Console.WriteLine($"{i + 1}) {teachers[keys[i]].GetInfoOneLine()}");
            Console.WriteLine("0) Отмена");
            int sel = ReadIntBetween(0, keys.Count);
            return sel == 0 ? -1 : keys[sel - 1];
        }

        public int ChooseCourse()
        {
            if (!courses.Any()) { Console.WriteLine("Курсы отсутствуют."); return -1; }
            var keys = courses.Keys.ToList();
            for (int i = 0; i < keys.Count; i++) Console.WriteLine($"{i + 1}) #{courses[keys[i]].Id} {courses[keys[i]].Title}");
            Console.WriteLine("0) Отмена");
            int sel = ReadIntBetween(0, keys.Count);
            return sel == 0 ? -1 : keys[sel - 1];
        }

        private static int ReadIntBetween(int min, int max)
        {
            while (true)
            {
                Console.Write($"Введите число ({min}-{max}): ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
                Console.WriteLine("Неверный ввод.");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var sys = new UniversitySystem();

            // Небольшие начальные данные
            var tid = sys.AddTeacher("Иван Петров", 45, "ivan@uni.ru");
            var cid = sys.AddCourse("Введение в C#", tid);
            var sid = sys.AddStudent("Анна Смирнова", 20, "anna@student.ru");
            sys.EnrollStudent(sid, cid);

            while (true)
            {
                Console.WriteLine("\n=== Главное меню ===");
                Console.WriteLine("1) Добавить");
                Console.WriteLine("2) Удалить");
                Console.WriteLine("3) Назначить преподавателя и/или записать студентов на курс");
                Console.WriteLine("4) Показать всё (одна строка на запись)");
                Console.WriteLine("0) Выход");
                Console.Write("Выберите пункт: ");
                var m = Console.ReadLine();

                try
                {
                    if (m == "0") { Console.WriteLine("Выход. До свидания!"); break; }

                    if (m == "1")
                    {
                        Console.WriteLine("Добавить: 1) Студента  2) Преподавателя  3) Курс  0) Отмена");
                        var choice = Console.ReadLine();
                        if (choice == "0") continue;
                        if (choice == "1")
                        {
                            Console.Write("Имя: "); var n = Console.ReadLine();
                            Console.Write("Возраст: "); var a = int.Parse(Console.ReadLine() ?? "0");
                            Console.Write("Контакт: "); var c = Console.ReadLine();
                            Console.WriteLine($"Добавлен студент ID = {sys.AddStudent(n, a, c)}");
                        }
                        else if (choice == "2")
                        {
                            Console.Write("Имя: "); var n = Console.ReadLine();
                            Console.Write("Возраст: "); var a = int.Parse(Console.ReadLine() ?? "0");
                            Console.Write("Контакт: "); var c = Console.ReadLine();
                            Console.WriteLine($"Добавлен преподаватель ID = {sys.AddTeacher(n, a, c)}");
                        }
                        else if (choice == "3")
                        {
                            Console.Write("Название курса: "); var t = Console.ReadLine();
                            Console.WriteLine("Выберите преподавателя для назначения (или 0):");
                            var tidSel = sys.ChooseTeacher();
                            int? tidOpt = tidSel == -1 ? (int?)null : tidSel;
                            Console.WriteLine($"Добавлен курс ID = {sys.AddCourse(t, tidOpt)}");
                        }
                    }
                    else if (m == "2")
                    {
                        Console.WriteLine("Удалить: 1) Студента  2) Преподавателя  3) Курс  0) Отмена");
                        var choice = Console.ReadLine();
                        if (choice == "0") continue;
                        if (choice == "1")
                        {
                            var id = sys.ChooseStudent();
                            if (id == -1) continue;
                            sys.RemoveStudent(id); Console.WriteLine("Студент удалён.");
                        }
                        else if (choice == "2")
                        {
                            var id = sys.ChooseTeacher();
                            if (id == -1) continue;
                            sys.RemoveTeacher(id); Console.WriteLine("Преподаватель удалён.");
                        }
                        else if (choice == "3")
                        {
                            var id = sys.ChooseCourse();
                            if (id == -1) continue;
                            sys.RemoveCourse(id); Console.WriteLine("Курс удалён.");
                        }
                    }
                    else if (m == "3")
                    {
                        Console.WriteLine("Выберите курс для операций:");
                        var cId = sys.ChooseCourse();
                        if (cId == -1) continue;

                        Console.WriteLine("Хотите назначить/поменять преподавателя? 1) Да  2) Нет");
                        var assign = Console.ReadLine();
                        if (assign == "1")
                        {
                            Console.WriteLine("Выберите преподавателя:");
                            var tId = sys.ChooseTeacher();
                            if (tId != -1) { sys.AssignTeacher(cId, tId); Console.WriteLine("Преподаватель назначен."); }
                        }

                        Console.WriteLine("Записать студентов на курс. Выберите студентов по очереди. Выберите 0 для завершения.");
                        while (true)
                        {
                            Console.WriteLine("Выберите студента для записи (или 0 для завершения):");
                            var sId = sys.ChooseStudent();
                            if (sId == -1) break;
                            try { sys.EnrollStudent(sId, cId); Console.WriteLine("Студент записан на курс."); }
                            catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
                        }
                    }
                    else if (m == "4")
                    {
                        sys.PrintAllDataOneLine();
                    }
                    else Console.WriteLine("Неверный пункт меню.");
                }
                catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
            }
        }
    }
}
