namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를선택하세요.";
                // 현재텍스트박스에있는경로를초기선택폴더로설정
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) &&
                Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwLeftDir, dlg.SelectedPath);
                    CompareAndColorize();
                }

            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를선택하세요.";
                // 현재텍스트박스에있는경로를초기선택폴더로설정
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) &&
                Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwRightDir, dlg.SelectedPath);
                    CompareAndColorize();
                }

            }
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            CopySelectedFiles(lvwRightDir, txtRightDir.Text, txtLeftDir.Text);
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            CopySelectedFiles(lvwLeftDir, txtLeftDir.Text, txtRightDir.Text);
        }

        private void PopulateListView(ListView lv, string folderPath)
        {
            lv.BeginUpdate();
            lv.Items.Clear();
            try
            {
                var dirs = Directory.EnumerateDirectories(folderPath)
                            .Select(p => new DirectoryInfo(p))
                            .OrderBy(d => d.Name);
                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add(d.LastWriteTime.ToString("g"));
                    lv.Items.Add(item);
                }
                var files = Directory.EnumerateFiles(folderPath)
                            .Select(p => new FileInfo(p))
                            .OrderBy(f => f.Name);
                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트");
                    item.SubItems.Add(f.LastWriteTime.ToString("g"));
                    lv.Items.Add(item);
                }
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(this, "폴더를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, "입출력 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lv.EndUpdate();
            }
        }

        private void CompareAndColorize()
        {
            var leftItems = lvwLeftDir.Items.Cast<ListViewItem>().ToDictionary(i => i.Text);
            var rightItems = lvwRightDir.Items.Cast<ListViewItem>().ToDictionary(i => i.Text);

            foreach (ListViewItem lItem in lvwLeftDir.Items)
            {
                if (rightItems.TryGetValue(lItem.Text, out ListViewItem rItem))
                {
                    DateTime lTime = DateTime.Parse(lItem.SubItems[2].Text);
                    DateTime rTime = DateTime.Parse(rItem.SubItems[2].Text);

                    if (lTime > rTime) lItem.ForeColor = Color.Red;
                    else if (lTime < rTime) lItem.ForeColor = Color.Gray;
                    else lItem.ForeColor = Color.Black;
                }
                else
                {
                    lItem.ForeColor = Color.Purple;
                }
            }

            foreach (ListViewItem rItem in lvwRightDir.Items)
            {
                if (leftItems.TryGetValue(rItem.Text, out ListViewItem lItem))
                {
                    DateTime rTime = DateTime.Parse(rItem.SubItems[2].Text);
                    DateTime lTime = DateTime.Parse(lItem.SubItems[2].Text);

                    if (rTime > lTime) rItem.ForeColor = Color.Red;
                    else if (rTime < lTime) rItem.ForeColor = Color.Gray;
                    else rItem.ForeColor = Color.Black;
                }
                else
                {
                    rItem.ForeColor = Color.Purple;
                }
            }
        }

        private void CopySelectedFiles(ListView sourceLv, string srcDir, string destDir)
        {
            if (string.IsNullOrEmpty(srcDir) || string.IsNullOrEmpty(destDir)) return;

            foreach (ListViewItem item in sourceLv.SelectedItems)
            {
                string fileName = item.Text;
                string sourcePath = Path.Combine(srcDir, fileName);
                string destPath = Path.Combine(destDir, fileName);

                if (File.Exists(sourcePath))
                {
                    if (File.Exists(destPath))
                    {
                        FileInfo srcInfo = new FileInfo(sourcePath);
                        FileInfo destInfo = new FileInfo(destPath);

                        if (destInfo.LastWriteTime > srcInfo.LastWriteTime)
                        {
                            string msg = "대상에 동일한 이름의 파일이 이미 있습니다.\n" +
                                         "대상 파일이 더 신규 파일입니다. 덮어쓰시겠습니까?";

                            if (MessageBox.Show(msg, "덮어쓰기 확인", MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                continue;
                            }
                        }
                    }

                    try
                    {
                        File.Copy(sourcePath, destPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류 발생 ({fileName}): " + ex.Message);
                    }
                }
            }

            PopulateListView(lvwLeftDir, txtLeftDir.Text);
            PopulateListView(lvwRightDir, txtRightDir.Text);
            CompareAndColorize();
        }
    }
}
