using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUniversity
{
    abstract class Person
    {
        public int Id;
        public string Name;
        public int Age;
        public string Contact;

        protected Person(int id, string name, int age, string contact)
        {
            Id = id;
            Name = name;
            Age = age;
            Contact = contact;
        }

        public abstract string OneLine();
    }

    class Student : Person
    {
        readonly HashSet<int> courses = new();
        public Student(int id, string name, int age, string contact) : base(id, name, age, contact) { }

        public void Enroll(int cid) => courses.Add(cid);
        public void Unenroll(int cid) => courses.Remove(cid);
        public IEnumerable<int> Courses => courses;

        public override string OneLine() =>
            $"Студент #{Id}: {Name}, {Age} лет, {Contact}; Курсы: {(courses.Count == 0 ? "—" : string.Join(",", courses))}";
    }

    class Teacher : Person
    {
        readonly HashSet<int> courses = new();
        public Teacher(int id, string name, int age, string contact) : base(id, name, age, contact) { }

        public void Assign(int cid) => courses.Add(cid);
        public void Unassign(int cid) => courses.Remove(cid);
        public IEnumerable<int> Courses => courses;

        public override string OneLine() =>
            $"Преподаватель #{Id}: {Name}, {Age} лет, {Contact}; Курсы: {(courses.Count == 0 ? "—" : string.Join(",", courses))}";
    }

    class Course
    {
        public int Id;
        public string Title;
        public int? TeacherId;
        readonly HashSet<int> students = new();

        public Course(int id, string title)
        {
            Id = id;
            Title = title;
        }

        public void SetTeacher(int? t) => TeacherId = t;
        public void AddStudent(int sid) => students.Add(sid);
        public void RemoveStudent(int sid) => students.Remove(sid);
        public IEnumerable<int> Students => students;

        public string OneLine()
        {
            var t = TeacherId.HasValue ? TeacherId.Value.ToString() : "не назначен";
            var s = students.Count == 0 ? "—" : string.Join(",", students);
            return $"Курс #{Id}: {Title}; Преподаватель: {t}; Студенты: {s}";
        }
    }

    class Univ
    {
        readonly Dictionary<int, Student> students = new();
        readonly Dictionary<int, Teacher> teachers = new();
        readonly Dictionary<int, Course> courses = new();
        int nextS = 1, nextT = 1, nextC = 1;

        public int AddStudent(string n, int a, string c) => AddEntity(students, id => new Student(id, n, a, c), ref nextS);
        public int AddTeacher(string n, int a, string c) => AddEntity(teachers, id => new Teacher(id, n, a, c), ref nextT);
        public int AddCourse(string title) => AddEntity(courses, id => new Course(id, title), ref nextC);

        static int AddEntity<T>(Dictionary<int, T> dict, Func<int, T> ctor, ref int next)
        {
            var id = next++;
            dict[id] = ctor(id);
            return id;
        }

        public void RemoveStudent(int id)
        {
            if (!students.Remove(id)) Console.WriteLine("Студент не найден");
            foreach (var c in courses.Values) c.RemoveStudent(id);
        }

        public void RemoveTeacher(int id)
        {
            if (!teachers.Remove(id)) Console.WriteLine("Преподаватель не найден");
            foreach (var c in courses.Values.Where(c => c.TeacherId == id)) c.SetTeacher(null);
        }

        public void RemoveCourse(int id)
        {
            if (!courses.TryGetValue(id, out var course)) Console.WriteLine("Курс не найден");
            foreach (var s in students.Values) s.Unenroll(id);
            if (course.TeacherId.HasValue && teachers.TryGetValue(course.TeacherId.Value, out var t))
                t.Unassign(id);
            courses.Remove(id);
        }

        public void AssignTeacherToCourse(int cid, int tid)
        {
            if (!courses.ContainsKey(cid)) Console.WriteLine("Курс не найден");
            if (!teachers.ContainsKey(tid)) Console.WriteLine("Преподаватель не найден");
            var course = courses[cid];
            if (course.TeacherId.HasValue)
                Console.Write("У курса уже есть преподаватель. Сначала удалите преподавателя или курс.");
            course.SetTeacher(tid);
            teachers[tid].Assign(cid);
        }

        public void EnrollStudentToCourse(int sid, int cid)
        {
            if (!students.ContainsKey(sid)) Console.WriteLine("Студент не найден");
            if (!courses.ContainsKey(cid)) Console.WriteLine("Курс не найден");
            students[sid].Enroll(cid);
            courses[cid].AddStudent(sid);
        }

        public void PrintAllOneLine()
        {
            Console.WriteLine("\nСтуденты");
            Console.WriteLine(string.Join(Environment.NewLine, students.Values.Select(s => s.OneLine())));
            Console.WriteLine("\nПреподаватели");
            Console.WriteLine(string.Join(Environment.NewLine, teachers.Values.Select(t => t.OneLine())));
            Console.WriteLine("\nКурсы");
            Console.WriteLine(string.Join(Environment.NewLine, courses.Values.Select(c => c.OneLine())));
            Console.WriteLine();
        }

        private int ReadIndex(int max, Func<int, int> mapper)
        {
            while (true)
            {
                Console.Write($"Введите номер (1-{max}) или 0 для отмены: ");
                if (int.TryParse(Console.ReadLine(), out int v))
                {
                    if (v == 0) return -1;
                    if (v >= 1 && v <= max) return mapper(v);
                }
                Console.WriteLine("Неверный ввод.");
            }
        }

        public int ChooseStudent() => Choose(ListStudents(), "студентов");
        public int ChooseTeacher() => Choose(ListTeachers(), "преподавателей");
        public int ChooseCourse() => Choose(ListCourses(), "курсов");

        private int Choose<T>(List<T> list, string title) where T : Person
        {
            if (list.Count == 0) { Console.WriteLine($"Нет {title}."); return -1; }
            for (int i = 0; i < list.Count; i++) Console.WriteLine($"{i + 1}) {list[i].OneLine()}");
            return ReadIndex(list.Count, i => list[i - 1].Id);
        }
        private int Choose(List<Course> list, string title)
        {
            if (list.Count == 0) { Console.WriteLine($"Нет {title}."); return -1; }
            for (int i = 0; i < list.Count; i++) Console.WriteLine($"{i + 1}) {list[i].OneLine()}");
            return ReadIndex(list.Count, i => list[i - 1].Id);
        }

        public List<Student> ListStudents() => students.Values.ToList();
        public List<Teacher> ListTeachers() => teachers.Values.ToList();
        public List<Course> ListCourses() => courses.Values.ToList();
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;
            var sys = new Univ();

            var t1 = sys.AddTeacher("Иван Петров", 45, "ivan@uni.ru");
            var t2 = sys.AddTeacher("Мария Соколова", 38, "maria@uni.ru");
            var t3 = sys.AddTeacher("Андрей Кузнецов", 50, "andrey@uni.ru");
            var t4 = sys.AddTeacher("Ольга Морозова", 42, "olga@uni.ru");
            var t5 = sys.AddTeacher("Дмитрий Орлов", 35, "dmitry@uni.ru");

            var c1 = sys.AddCourse("Введение в C#");
            var c2 = sys.AddCourse("Базы данных");
            var c3 = sys.AddCourse("Алгоритмы и структуры данных");
            var c4 = sys.AddCourse("Веб-разработка");
            var c5 = sys.AddCourse("Машинное обучение");

            var s1 = sys.AddStudent("Анна Смирнова", 20, "anna@student.ru");
            var s2 = sys.AddStudent("Павел Иванов", 21, "pavel@student.ru");
            var s3 = sys.AddStudent("Екатерина Орлова", 19, "ekaterina@student.ru");
            var s4 = sys.AddStudent("Михаил Сергеев", 22, "mikhail@student.ru");
            var s5 = sys.AddStudent("Юлия Кузьмина", 20, "yulia@student.ru");

            while (true)
            {
                Console.WriteLine("\nМеню");
                Console.WriteLine("1 - Добавить");
                Console.WriteLine("2 - Удалить");
                Console.WriteLine("3 - Назначить преподавателя на курс");
                Console.WriteLine("4 - Записать студентов на курс");
                Console.WriteLine("5 - Показать всё");
                Console.WriteLine("0 - Выход");
                Console.Write("Выбор: ");
                var cmd = Console.ReadLine()?.Trim();

                try
                {
                    switch (cmd)
                    {
                        case "0":
                            return;

                        case "1":
                            Console.WriteLine("1) Студент  2) Преподаватель  3) Курс  0) Отмена");
                            var ch1 = Console.ReadLine();
                            switch (ch1)
                            {
                                case "1":
                                    Console.Write("Имя: "); var n = Console.ReadLine();
                                    Console.Write("Возраст: "); var a = int.Parse(Console.ReadLine() ?? "0");
                                    Console.Write("Контакт: "); var co = Console.ReadLine();
                                    Console.WriteLine($"Добавлен студент ID = {sys.AddStudent(n ?? "", a, co ?? "")}");
                                    break;
                                case "2":
                                    Console.Write("Имя: "); n = Console.ReadLine();
                                    Console.Write("Возраст: "); a = int.Parse(Console.ReadLine() ?? "0");
                                    Console.Write("Контакт: "); co = Console.ReadLine();
                                    Console.WriteLine($"Добавлен преподаватель ID = {sys.AddTeacher(n ?? "", a, co ?? "")}");
                                    break;
                                case "3":
                                    Console.Write("Название курса: "); var title = Console.ReadLine();
                                    Console.WriteLine($"Добавлен курс ID = {sys.AddCourse(title ?? "")}");
                                    break;
                                default:
                                    Console.WriteLine("Отмена.");
                                    break;
                            }
                            break;

                        case "2":
                            Console.WriteLine("1) Удалить студента  2) Преподавателя  3) Курс  0) Отмена");
                            var ch2 = Console.ReadLine();
                            switch (ch2)
                            {
                                case "1":
                                    var id = sys.ChooseStudent();
                                    if (id != -1) { sys.RemoveStudent(id); Console.WriteLine("Студент удалён."); }
                                    break;
                                case "2":
                                    id = sys.ChooseTeacher();
                                    if (id != -1) { sys.RemoveTeacher(id); Console.WriteLine("Преподаватель удалён; курсы отвязаны."); }
                                    break;
                                case "3":
                                    id = sys.ChooseCourse();
                                    if (id != -1) { sys.RemoveCourse(id); Console.WriteLine("Курс удалён."); }
                                    break;
                                default:
                                    Console.WriteLine("Отмена.");
                                    break;
                            }
                            break;

                        case "3":
                            Console.WriteLine("Выберите курс для назначения преподавателя:");
                            var cid = sys.ChooseCourse();
                            if (cid == -1) break;
                            Console.WriteLine("Выберите преподавателя:");
                            var tid = sys.ChooseTeacher();
                            if (tid == -1) break;
                            sys.AssignTeacherToCourse(cid, tid);
                            Console.WriteLine("Преподаватель назначен.");
                            break;

                        case "4":
                            Console.WriteLine("Выберите курс для записи студентов:");
                            cid = sys.ChooseCourse();
                            if (cid == -1) break;
                            Console.WriteLine("Выбирайте студентов. 0 — завершить.");
                            var sid = sys.ChooseStudent();
                            if (sid == -1) break;
                            sys.EnrollStudentToCourse(sid, cid);
                            Console.WriteLine("Студент записан.");

                            break;

                        case "5":
                            sys.PrintAllOneLine();

                            break;

                        default:
                            Console.WriteLine("Неверный пункт меню.");
                            break;
                    }
                }
                catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
            }
        }
    }
}
